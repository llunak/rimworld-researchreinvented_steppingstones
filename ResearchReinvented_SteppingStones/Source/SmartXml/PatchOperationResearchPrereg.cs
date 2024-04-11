﻿using PeteTimesSix.ResearchReinvented_SteppingStones.SmartXml;
using PeteTimesSix.ResearchReinvented_SteppingStones.Utilities;
using RR.SmartXml.Conditionals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace RR
{
    public enum TargetPrerequisiteKind
    {
        ProjectPrerequisites,
        ProjectHiddenPrerequisites,
        Prerequisites,
        SowResearchPrerequisites
    }

    public class PatchOperationResearchPrereg : PatchOperationPathed
    {
        public Type conditionalType = null;
        public string conditionalParam = null;
        public string doesRequire;

        public string target;
        public List<string> defNames;

        bool clearFirst = true;

        public override void Complete(string modIdentifier)
        {
            base.Complete(modIdentifier);
        }

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (!string.IsNullOrWhiteSpace(doesRequire)) 
            {
                var split = doesRequire.Split(',');
                foreach(var mod in split)
                {
                    if (!ModsConfig.IsActive(mod.Trim()))
                    {
                        return true; //a required mod isnt loaded, so we dont apply the patch
                    }
                }
            }

            if(conditionalType != null)
            {
                var conditional = SmartXmlConditionalMaker.MakeConditional(conditionalType);
                if(!conditional.ShouldExecute(conditionalParam))
                    return true; //specific condition (probably a settings value) not met
			}

            bool matched = false;
            foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
            {
                matched = true;
                XmlNode targetNode = null;
                foreach(XmlNode node in xmlNode.ChildNodes)
                {
                    if(node.Name == target)
                    {
                        targetNode = node;
                        break;
                    }
                }
                if (targetNode == null)
                {
                    targetNode = xmlNode.OwnerDocument.CreateElement(target);
                    xmlNode.AppendChild(targetNode);
                }
                if(clearFirst)
                {
                    targetNode.RemoveAll();
                }

                foreach (var defName in defNames)
                {
                    var toAdd = DefNameSubstitutor.GetDefNameOrSub(defName);

                    if (!clearFirst)
                    {
                        bool alreadyPresent = false;
                        foreach (XmlNode childNode in targetNode.ChildNodes)
                        {
                            if (childNode.InnerText == toAdd)
                            {
                                alreadyPresent = true;
                                break;
                            }
                        }
                        if (alreadyPresent)
                            continue;
                    }

                    XmlNode newNode = targetNode.OwnerDocument.CreateElement("li");
                    newNode.InnerText = toAdd;
                    targetNode.AppendChild(newNode);
                }
            }
            return matched;
        }
    }
}
