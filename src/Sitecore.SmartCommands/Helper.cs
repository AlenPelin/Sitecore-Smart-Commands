using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SmartCommands
{
    public static class Helper
    {
        /// <summary>
        /// Replace item references (Datasource links) with new ones that are descendant of it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="copy"></param>
        /// <param name="deep"></param>
        /// <param name="isAsync"></param>
        public static void ReplaceItemReferences(Item source, Item copy, bool deep, bool isAsync)
        {
            Sitecore.Pipelines.CorePipeline.Run("replaceItemReferences", new Sitecore.Pipelines.ReplaceItemReferences.ReplaceItemReferencesArgs()
            {
                SourceItem = source,
                CopyItem = copy,
                Deep = deep,
                Async = isAsync
            });
        }

        /// <summary>
        /// Utility method for relinking branch datasources
        /// </summary>
        public static void RelinkDatasourcesInBranchInstance(Item item, bool descendants = false)
        {
            RelinkDatasourcesForItemInBranchInstance(item, item);

            if (!descendants)
            {
                return;
            }

            foreach (var descendant in item.Axes.GetDescendants())
            {
                RelinkDatasourcesForItemInBranchInstance(descendant, item);
            }
        }

        /// <summary>
        /// Utility method for relinking datasources for an item within a branch instance
        /// </summary>
        /// <remarks>
        /// Adapted from original code, written by Kam Figy: 
        /// https://github.com/kamsar/BranchPresets/blob/master/BranchPresets/AddFromBranchPreset.cs
        /// </remarks>
        public static void RelinkDatasourcesForItemInBranchInstance(Item item, Item instanceRoot)
        {
            Action<RenderingDefinition> relinkRenderingDatasource =
                rendering =>
                    RelinkRenderingDatasourceForItemInBranch(item, instanceRoot, rendering);

            ApplyActionToAllRenderings(item, relinkRenderingDatasource);
        }


        /// <summary>
        /// Utility method for relinking the datasource for the supplied rendering on an item in the branch instance
        /// </summary>
        /// <remarks>
        /// Adapted from original code, written by Kam Figy: 
        /// https://github.com/kamsar/BranchPresets/blob/master/BranchPresets/AddFromBranchPreset.cs
        /// </remarks>
        /// <param name="item">Item that contains the rendering</param>
        /// <param name="instanceRoot">Root item of the branch instance</param>
        /// <param name="rendering">Rendering to be relinked</param>
        public static void RelinkRenderingDatasourceForItemInBranch(Item item, Item instanceRoot, RenderingDefinition rendering)
        {
            var branchBasePath = item.Branch.InnerItem.Paths.FullPath;

            if (string.IsNullOrWhiteSpace(rendering.Datasource))
            {
                return;
            }

            var database = item.Database;

            // note: queries and multiple item datasources are not supported
            var renderingTargetItem = database.GetItem(rendering.Datasource);

            Assert.IsNotNull(
                renderingTargetItem,
                String.Format("Error while expanding branch template rendering datasources: data source {0} was not resolvable.", rendering.Datasource)
                    );

            // if there was no valid target item OR the target item is not a child of the branch template we skip out
            if (renderingTargetItem == null ||
                !renderingTargetItem.Paths.FullPath.StartsWith(branchBasePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // get the path relative to the branch item
            var relativeRenderingPath =
                renderingTargetItem.Paths.FullPath.Substring(branchBasePath.Length);

            // replace $name Sitecore tokens in path
            relativeRenderingPath = relativeRenderingPath
                .Replace("$name", instanceRoot.Name);

            var newTargetPath = instanceRoot.Parent.Paths.FullPath + relativeRenderingPath;
            var newTargetItem = database.GetItem(newTargetPath);

            // if the target item was a valid under branch item, but the same relative path does not exist under the branch instance
            // we set the datasource to something invalid to avoid any potential unintentional edits of a shared data source item
            rendering.Datasource = newTargetItem?.ID.ToString() ?? "INVALID_BRANCH_SUBITEM_ID";
        }

        /// <summary>
        ///     Invokes Action on all Renderings on item
        /// </summary>
        public static void ApplyActionToAllRenderings(Item item, Action<RenderingDefinition> action)
        {
            ApplyActionToAllSharedRenderings(item, action);
            ApplyActionToAllFinalRenderings(item, action);
        }

        /// <summary>
        ///     Invokes Action on all Shared Renderings on item
        /// </summary>
        public static void ApplyActionToAllSharedRenderings(Item item, Action<RenderingDefinition> action)
        {
            ApplyActionToLayoutField(item, FieldIDs.LayoutField, action);
        }

        /// <summary>
        ///     Invokes Action on all Final Renderings on item
        /// </summary>
        public static void ApplyActionToAllFinalRenderings(Item item, Action<RenderingDefinition> action)
        {
            ApplyActionToLayoutField(item, FieldIDs.FinalLayoutField, action);
        }

        /// <summary>
        ///     Invokes Action on all Final Renderings on item
        /// </summary>
        private static void ApplyActionToLayoutField(Item item, ID fieldId, Action<RenderingDefinition> action)
        {
            var currentLayoutXml = LayoutField.GetFieldValue(item.Fields[fieldId]);
            if (string.IsNullOrEmpty(currentLayoutXml))
            {
                return;
            }

            var newXml = ApplyActionToLayoutXml(currentLayoutXml, action);
            if (newXml == null)
            {
                return;
            }

            using (new SecurityDisabler())
            {
                using (new EditContext(item))
                {
                    // NOTE: when dealing with layouts its important to get and set the field value with LayoutField.Get/SetFieldValue()
                    // if you fail to do this you will not process layout deltas correctly and may instead override all fields (breaking full inheritance), 
                    // or attempt to get the layout definition for a delta value, which will result in your wiping the layout details when they get saved.
                    LayoutField.SetFieldValue(item.Fields[fieldId], newXml);
                }
            }
        }

        private static string ApplyActionToLayoutXml(string xml, Action<RenderingDefinition> action)
        {
            var layout = LayoutDefinition.Parse(xml);

            // normalize the output in case of any minor XML differences (spaces, etc)
            xml = layout.ToXml();

            // loop over devices in the rendering
            for (var deviceIndex = layout.Devices.Count - 1; deviceIndex >= 0; deviceIndex--)
            {
                var device = layout.Devices[deviceIndex] as DeviceDefinition;
                if (device == null)
                {
                    continue;
                }

                // loop over renderings within the device
                for (var renderingIndex = device.Renderings.Count - 1; renderingIndex >= 0; renderingIndex--)
                {
                    var rendering = device.Renderings[renderingIndex] as RenderingDefinition;
                    if (rendering == null)
                    {
                        continue;
                    }

                    // run the action on the rendering
                    action(rendering);
                }
            }

            var layoutXml = layout.ToXml();

            // save a modified layout value if necessary
            return layoutXml != xml ? layoutXml : null;
        }
    }
}