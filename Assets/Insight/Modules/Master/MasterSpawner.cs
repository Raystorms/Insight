﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Insight
{
    public class MasterSpawner : InsightModule
    {
        [HideInInspector] public InsightServer server;

        public List<SpawnerContainer> registeredSpawners = new List<SpawnerContainer>();

        public override void Initialize(InsightServer insight, ModuleManager manager)
        {
            server = insight;
            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            server.RegisterHandler((short)MsgId.RegisterSpawner, HandleRegisterSpawnerMsg);
            server.RegisterHandler((short)MsgId.RequestSpawnStart, HandleSpawnRequestMsg);
            server.RegisterHandler((short)MsgId.SpawnerStatus, HandleSpawnerStatusMsg);
        }

        private void HandleRegisterSpawnerMsg(InsightNetworkMessage netMsg)
        {
            RegisterSpawnerMsg message = netMsg.ReadMessage<RegisterSpawnerMsg>();

            //Add the new child spawner to the list of spawners
            registeredSpawners.Add(new SpawnerContainer()
            {
                uniqueId = message.UniqueID,
                connectionId = netMsg.connectionId,
                MaxThreads = message.MaxThreads
            });

            if (server.logNetworkMessages) { Debug.Log("[MasterSpawner] - New Process Spawner Regsitered"); }
        }

        //Instead of handling the msg here we will forward it to an available spawner.
        private void HandleSpawnRequestMsg(InsightNetworkMessage netMsg)
        {
            RequestSpawnStartMsg message = netMsg.ReadMessage<RequestSpawnStartMsg>();

            //Get all spawners that have atleast 1 slot free
            List<SpawnerContainer> freeSlotSpawners = new List<SpawnerContainer>();
            foreach (SpawnerContainer spawner in registeredSpawners)
            {
                if (spawner.CurrentThreads < spawner.MaxThreads)
                {
                    freeSlotSpawners.Add(spawner);
                }
            }

            //sort by least busy spawner first
            freeSlotSpawners = freeSlotSpawners.OrderBy(x => x.CurrentThreads).ToList();
            server.SendToClient(freeSlotSpawners[0].connectionId, (short)MsgId.RequestSpawnStart, message, (callbackStatus, reader) =>
            {
                if (callbackStatus == CallbackStatus.Ok)
                {
                    RequestSpawnStartMsg callbackResponse = reader.ReadMessage<RequestSpawnStartMsg>();
                    if (server.logNetworkMessages) { Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID); }

                //If callback from original message is present
                if (netMsg.callbackId != 0)
                    {
                        netMsg.Reply((short)MsgId.RequestSpawnStart, callbackResponse);
                    }
                }
                if (callbackStatus == CallbackStatus.Timeout)
                {
                    RequestSpawnStartMsg callbackResponse = reader.ReadMessage<RequestSpawnStartMsg>();
                    Debug.Log("[Spawn Callback] Createion Timed Out: " + callbackResponse.UniqueID);
                }
                if (callbackStatus == CallbackStatus.Error)
                {
                    Debug.Log("[Spawn Callback] Error in SpawnRequest.");
                }
            });
        }

        private void HandleSpawnerStatusMsg(InsightNetworkMessage netMsg)
        {
            SpawnerStatusMsg message = netMsg.ReadMessage<SpawnerStatusMsg>();

            for (int i = 0; i < registeredSpawners.Count; i++)
            {
                if (registeredSpawners[i].connectionId == netMsg.connectionId)
                {
                    SpawnerContainer instance = registeredSpawners[i];
                    instance.CurrentThreads = message.CurrentThreads;
                }
            }
        }

        public void InternalSpawnRequest(RequestSpawnStartMsg message)
        {
            //Get all spawners that have atleast 1 slot free
            List<SpawnerContainer> freeSlotSpawners = registeredSpawners.Where(x => (x.CurrentThreads < x.MaxThreads)).ToList();

            if (freeSlotSpawners.Count == 0)
            {
                Debug.LogError("[MasterSpawner] - No Spawners with slots free available to service SpawnRequest.");
                return;
            }

            //sort by least busy spawner first
            freeSlotSpawners = freeSlotSpawners.OrderBy(x => x.CurrentThreads).ToList();

            //Send SpawnRequest to the least busy Spawner
            server.SendToClient(freeSlotSpawners[0].connectionId, (short)MsgId.RequestSpawnStart, message);
        }
    }

    [Serializable]
    public struct SpawnerContainer
    {
        public string uniqueId;
        public int connectionId;
        public int MaxThreads;
        public int CurrentThreads;
    }
}