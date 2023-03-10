using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BranchMaker
{
    public class BranchNodeBlock
    {
        public string id;
        public string dialogue;
        public string nickname;
        public string meta_scripts;
        public string voice_file;
        public string character;
        public string emotion;

        // Action nodes
        public string clean_action;
        public string target_node;

        public bool processed;

        // Lingual
        public string process_lines;
        public string process_subtle_lines;
        public List<string> process_line_list;
        public List<string> script_lines = new List<string>();
        public List<string> processed_checklist = new List<string>();

        public static BranchNodeBlock createFromJson(SimpleJSON.JSONNode jsonNode)
        {
            BranchNodeBlock block = new BranchNodeBlock();
            block.id = jsonNode["id"].Value;
            block.nickname = jsonNode["nickname"].Value;
            block.dialogue = jsonNode["dialogue"].Value;
            block.voice_file = jsonNode["voice_file"].Value;
            block.meta_scripts = jsonNode["meta_scripts"].Value.ToLower();
            block.character = (jsonNode["character"].Value.ToLower() ?? null);
            block.emotion = (jsonNode["emotion"].Value.ToLower() ?? null);

            if (block.meta_scripts != string.Empty) {
                block.script_lines = new List<string>();
                foreach (string bit in block.meta_scripts.ToLower().Trim().Split('\n'))
                {
                    block.script_lines.Add(bit.Trim());
                }
            }

            // Action nodes
            if (jsonNode["target_node"] != null)
            {
                block.target_node = jsonNode["target_node"].Value;
                block.clean_action = jsonNode["dialogue"].Value.Trim().ToLower();
                // Lingual
                block.process_lines = jsonNode["processed_lines"].Value.ToLower();
                block.process_line_list = new List<string>();
                foreach (string bit in block.process_lines.ToLower().Trim().Split('\n')) {
                    block.process_line_list.Add(bit.Trim());
                }
                block.process_subtle_lines = jsonNode["processed_subtle_lines"].Value.ToLower();


            }
            return block;
        }

        public bool matchesSegmentationFlag(string[] segments, string currentSegment)
        {
            foreach (string mk in segments)
            {
                if (script_lines.Contains(mk))
                {
                    return false;
                }
                if (script_lines.Contains("!" + currentSegment))
                {
                    return false;
                }
            }
            return true;
        }

        public bool isActionNode() {
            return (target_node != null);
        }

        public bool HasMetaScript()
        {
            if (string.IsNullOrEmpty(meta_scripts)) return false;
            if (meta_scripts == "null") return false;
            return true;
        }

        public IEnumerable<string> MetaScriptLines()
        {
            return meta_scripts.Trim().Split("\n"[0]);
        }
    }
}
