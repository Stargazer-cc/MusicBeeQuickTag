using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicBeePlugin
{
    public class PresetGroup
    {
        public string Name { get; set; }
        public List<string> Tags { get; set; }

        public PresetGroup(string name)
        {
            Name = name;
            Tags = new List<string>();
        }
    }

    public static class PresetManager
    {
        public static List<PresetGroup> Load(string path)
        {
            List<PresetGroup> groups = new List<PresetGroup>();
            if (!File.Exists(path)) return groups;

            string[] lines = File.ReadAllLines(path);
            PresetGroup currentGroup = null;

            // Default group for backward compatibility or lines before any header
            PresetGroup defaultGroup = new PresetGroup("Default");
            bool hasNamedGroups = false;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                // Check for [GroupName] header
                Match match = Regex.Match(trimmed, @"^\[(.*)\]$");
                if (match.Success)
                {
                    string name = match.Groups[1].Value.Trim();
                    currentGroup = new PresetGroup(name);
                    groups.Add(currentGroup);
                    hasNamedGroups = true;
                }
                else
                {
                    if (currentGroup == null)
                    {
                        currentGroup = defaultGroup;
                    }
                    currentGroup.Tags.Add(trimmed);
                }
            }

            // Handle legacy files (no headers)
            if (!hasNamedGroups && defaultGroup.Tags.Count > 0)
            {
                groups.Add(defaultGroup);
            }
            // If we had named groups but some loose lines at start, add default group if it has content
            else if (hasNamedGroups && defaultGroup.Tags.Count > 0)
            {
                groups.Insert(0, defaultGroup);
            }

            if (groups.Count == 0)
            {
                groups.Add(new PresetGroup("Default"));
            }

            return groups;
        }

        public static void Save(string path, List<PresetGroup> groups)
        {
            List<string> lines = new List<string>();
            foreach (var group in groups)
            {
                lines.Add("[" + group.Name + "]");
                lines.AddRange(group.Tags);
                lines.Add(""); // Empty line between groups
            }
            File.WriteAllLines(path, lines.ToArray());
        }
    }
}
