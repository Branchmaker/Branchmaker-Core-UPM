﻿using System.Collections.Generic;

namespace BranchMaker {
    public class BranchNode
    {
        public string id;
        public string nickname;
        public List<BranchNodeBlock> blocks = new();
        public List<BranchNodeBlock> suggestions = new();

        public bool processed;

        public List<BranchNodeBlock> StoryBlocks() => blocks.FindAll(a => !a.isActionNode());
        public List<BranchNodeBlock> ActionBlocks() => blocks.FindAll(a => a.isActionNode());

        static public Dictionary<string, BranchNode> nodecollection = new();

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
                nodeblock.node_id = node.id;
                node.blocks.Add(nodeblock);
            }

            nodecollection[node.id] = node;

            return node;
        }
    }
}
