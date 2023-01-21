namespace SpaceRpg.UnityServices.Relay
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Services.Relay;
    using Unity.Services.Relay.Models;
    using System;
    using System.Linq;

    public static class UnityRelayUtilities
    {
        const string k_DtlsConnType = "dtls";

        public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[] key, string joinCode)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
        {
            Allocation allocation;
            string joinCode;

            try
            {
                allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch(Exception ex)
            {
                throw new Exception($"Creating allocation request has failed: \n {ex.Message}", ex);
            }

            Debug.Log($"Server: Connection Data: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}, Allocation ID: {allocation.AllocationId}, region:{allocation.Region}");

            try
            {
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch(Exception ex)
            {
                throw new Exception($"Creating join code request has failed: \n {ex.Message}", ex);
            }

            var dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == k_DtlsConnType);

            return (dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.Key, joinCode);
        }

        public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, Guid allocationId, byte[] connectionData, byte[] hostConnectionData, byte[] key)> JoinRelayServerFromJoinCode(string joinCode)
        {
            JoinAllocation allocation;

            try
            {
                allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Creating join code request has failed: \n {ex.Message}", ex);
            }

            Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
            Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
            Debug.Log($"client: {allocation.AllocationId}");

            var dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == k_DtlsConnType);
            return (dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.AllocationId,
                allocation.ConnectionData, allocation.HostConnectionData, allocation.Key);
        }
    }
}