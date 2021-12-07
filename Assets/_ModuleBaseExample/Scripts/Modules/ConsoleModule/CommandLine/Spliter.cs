using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    public class CommandSpliter
    {
        public static string[] Split(string line)
        {   
            bool inQuota = false;
            List<string> splited = new List<string>();
            int index = 0;
            for(int i = 0; i < line.Length; i++)
            {
                if(line[i] == '"')
                {
                    inQuota = !inQuota;
                }
                if(line[i] == ' ' && !inQuota)
                {
                    int count = i - index;
                    if (count < 1)
                    {
                        index = i + 1;
                        continue;
                    }    
                    var sub = line.Substring(index, i - index);
                    splited.Add(sub);
                    index = i + 1;
                }
            }
            if(index < line.Length)
            {
                splited.Add(line.Substring(index, line.Length - index));
            }
            return splited.ToArray();
        }
    }

}
