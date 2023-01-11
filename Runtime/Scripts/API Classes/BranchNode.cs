using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker {
    public class BranchNode
    {
        public string id;
        public string nickname;
        public List<BranchNodeBlock> blocks = new List<BranchNodeBlock>();

        public bool processed;

        public List<BranchNodeBlock> StoryBlocks() => blocks.FindAll(a => !a.isActionNode());
        public List<BranchNodeBlock> ActionBlocks() => blocks.FindAll(a => a.isActionNode());

        static public Dictionary<string, BranchNode> nodecollection = new Dictionary<string, BranchNode>();

        // Use this for initialization
        public static BranchNode createFromJson(SimpleJSON.JSONNode jsonNode)
        {
            var node = new BranchNode
            {
                id = jsonNode["id"].Value,
                nickname = jsonNode["nickname"].Value
            };

            foreach (SimpleJSON.JSONNode block in jsonNode["blocks"]) {
                var nodeblock = BranchNodeBlock.createFromJson(block);
                node.blocks.Add(nodeblock);
            }

            if (!nodecollection.ContainsKey(node.id)) nodecollection.Add(node.id, node);
            else nodecollection[node.id] = node;


            return node;
        }
    }
}
