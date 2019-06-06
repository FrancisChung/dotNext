﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotNext.Threading;

namespace DotNext.Net.Cluster.Consensus.Raft.Http
{
    internal sealed class RaftClusterMember : HttpClient, IRaftClusterMember
    {
        private const string UserAgent = "Raft.NET";

        private delegate Task<RaftHttpMessage<TBody>.Response> ResponseParser<TBody>(HttpResponseMessage response);

        private const int UnknownStatus = (int) ClusterMemberStatus.Unknown;
        private const int UnavailableStatus = (int) ClusterMemberStatus.Unavailable;
        private const int AvailableStatus = (int) ClusterMemberStatus.Available;

        private readonly Uri resourcePath;
        private int status;
        private readonly ISite owner;
        private volatile MemberMetadata metadata;
        private Guid id;

        internal RaftClusterMember(ISite owner, Uri remoteMember, Uri resourcePath)
        {
            this.resourcePath = resourcePath;
            this.owner = owner;
            status = UnknownStatus;
            BaseAddress = remoteMember;
            switch (remoteMember.HostNameType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    Endpoint = new IPEndPoint(IPAddress.Parse(remoteMember.Host), remoteMember.Port);
                    break;
                default:
                    throw new UriFormatException(ExceptionMessages.UnresolvedHostName(remoteMember.Host));
            }
            DefaultRequestHeaders.ConnectionClose = true;   //to avoid network storm
            DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(UserAgent, GetType().Assembly.GetName().Version.ToString()));
        }

        internal bool IsLocal => owner.LocalMember.Id.Equals(id);

        private void ChangeStatus(int newState)
        {
            var previousState = status.GetAndSet(newState);
            if(previousState != newState)
                owner.MemberStatusChanged(this, (ClusterMemberStatus) previousState, (ClusterMemberStatus) newState);
        }

        private async Task<TBody> ParseResponse<TBody>(HttpResponseMessage response, ResponseParser<TBody> parser)
        {
            var reply = await parser(response).ConfigureAwait(false);
            id = reply.MemberId;
            return reply.Body;
        }

        //null means that node is unreachable
        //true means that node votes successfully for the new leader
        //false means that node is in candidate state and rejects voting
        public async Task<bool?> VoteAsync(CancellationToken token)
        {
            if (IsLocal)
                return true;
            var request = (HttpRequestMessage) new RequestVoteMessage(owner);
            request.RequestUri = resourcePath;
            var response = default(HttpResponseMessage);
            try
            {
                response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
                var result = await ParseResponse<bool>(response, RequestVoteMessage.GetResponse).ConfigureAwait(false);
                ChangeStatus(AvailableStatus);
                return result;
            }
            catch (HttpRequestException)
            {
                ChangeStatus(UnavailableStatus);
                return null;
            }
            finally
            {
                response?.Dispose();
                request.Dispose();
            }
        }

        public async Task<bool> ResignAsync(CancellationToken token)
        {
            var request = (HttpRequestMessage) new ResignMessage(owner.LocalMember);
            request.RequestUri = resourcePath;
            var response = default(HttpResponseMessage);
            try
            {
                response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, token)
                    .ConfigureAwait(false);
                var result = await ParseResponse<bool>(response, ResignMessage.GetResponse).ConfigureAwait(false);
                ChangeStatus(AvailableStatus);
                return result;
            }
            catch (HttpRequestException)
            {
                ChangeStatus(UnavailableStatus);
                return false;
            }
            finally
            {
                response?.Dispose();
                request.Dispose();
            }
        }

        async ValueTask<IReadOnlyDictionary<string, string>> IClusterMember.GetMetadata(bool refresh, CancellationToken token)
        {
            if (IsLocal)
                return owner.LocalMember.Metadata;
            else if (metadata is null || refresh)
            {
                var request = (HttpRequestMessage) new MetadataMessage(owner.LocalMember);
                request.RequestUri = resourcePath;
                var response = default(HttpResponseMessage);
                try
                {
                    response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                        .ConfigureAwait(false);
                    metadata = await ParseResponse<MemberMetadata>(response, MetadataMessage.GetResponse)
                        .ConfigureAwait(false);
                    ChangeStatus(AvailableStatus);
                }
                catch (HttpRequestException)
                {
                    ChangeStatus(UnavailableStatus);
                    throw;
                }
                finally
                {
                    response?.Dispose();
                    request.Dispose();
                }
            }

            return metadata;
        }

        public IPEndPoint Endpoint { get; }
        bool IClusterMember.IsLeader => owner.IsLeader(this);

        bool IClusterMember.IsRemote => !IsLocal;

        ClusterMemberStatus IClusterMember.Status => (ClusterMemberStatus) status.VolatileRead();

        bool IEquatable<IClusterMember>.Equals(IClusterMember other) => Endpoint.Equals(other?.Endpoint);
    }
}