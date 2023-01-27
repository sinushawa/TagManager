using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using dragonz.actb.core;
using dragonz.actb.provider;

namespace TagManager
{
    public partial class FastWPFTag : System.Windows.Controls.UserControl, IComponentConnector
    {
        private List<TagNode> nodesList;
        private List<string> branchNames;
        private TagNode projectEntity;
        private bool consoleMode = false;
        private bool shortcutMode = false;
        private ConsoleContainerElement _consoleRoot;
        private ConsoleContainerElement _currentContainer;
        public Window winParent;

        public FastWPFTag()
        {
            this.InitializeComponent();
            
        }


        public void CreateAutoCompleteSource()
        {
            projectEntity = TagGlobals.root.GetNodeList().First(x => x.Name == "Project");
            nodesList = projectEntity.Children.ToList().GetNodeList();
            branchNames = new List<string>();
            foreach (TagNode _entity in nodesList)
            {
                string _branchName = _entity.GetNodeBranchName(TagGlobals.delimiter, TagGlobals.baseNames);
                branchNames.Add(_branchName);
            }
            actbFastBox.AutoCompleteManager.DataProvider = new DataProviderContains(branchNames);
            actbFastBox.AutoCompleteManager.AutoAppend = false;
            _consoleRoot = new ConsoleContainerElement();
            _currentContainer = _consoleRoot;
        }

        private void FastBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            dragonz.actb.control.AutoCompleteTextBox autoCompleteBox = (dragonz.actb.control.AutoCompleteTextBox)sender;

            // open bracket [
            if (e.Key == Key.Oem4)
            {
                FastPop.IsOpen = true;
                ConsoleContainerElement consoleContainerElement = new ConsoleContainerElement(_currentContainer);
                _currentContainer.content.Add(consoleContainerElement);
                _currentContainer = consoleContainerElement;
                e.Handled = true;
            }

            // close bracket ]
            if (e.Key == Key.Oem6)
            {
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                }
                autoCompleteBox.Text = "";
                _currentContainer = _currentContainer.parent;
                e.Handled = true;
            }

            // + key
            if (e.Key == Key.Add)
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                    _currentContainer.ops.Add(concat.addition);
                }
                else
                {
                    _currentContainer.ops.Add(concat.addition);
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }

            // - key
            if (e.Key == Key.Subtract)
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null && autoCompleteBox.Text != "")
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                    _currentContainer.ops.Add(concat.substraction);
                }
                else
                {
                    _currentContainer.ops.Add(concat.substraction);
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }

            // slash key /
            if (e.Key == Key.Oem2)
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity == null)
                {
                    entity = TagHelperMethods.GetLonguestMatchingTag(autoCompleteBox.Text, true, null);
                }
                if (!entity.IsShortcut)
                {
                    TagMethods.ApplyEntities(new List<TagNode>() { entity }, MaxPluginUtilities.Selection.ToListHandles());
                    if (TagGlobals.autoRename && entity.IsNameable)
                    {
                        TagMethods.RenameUsingStructure();
                    }
                }
                winParent.Close();
            }

            // backslash key \
            if (e.Key == Key.Oem5)
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity == null)
                {
                    entity = TagHelperMethods.GetLonguestMatchingTag(autoCompleteBox.Text, true, false);
                }
                if (!entity.IsShortcut)
                {
                    TagMethods.ApplyEntities(new List<TagNode>() { entity }, MaxPluginUtilities.Selection.ToListHandles());
                }
                winParent.Close();
            }

            // enter key 
            if (e.Key == Key.Return)
            {
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (shortcutMode)
                {
                    entity = TagHelperMethods.GetLonguestMatchingTag(autoCompleteBox.Text, false, null);
                    TagNode _shortcut = new TagNode(autoCompleteBox.Text, _consoleRoot);
                    entity.Children.Add(_shortcut);
                }
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                }

                List<uint> _nodeHandles = _currentContainer.getCorrespondingSel();

                TagGlobals.selectionChain.Push(TagMethods.GetEntitiesContainingObjects(_nodeHandles).ToList());

                MaxPluginUtilities.SetSelection(_nodeHandles);
                autoCompleteBox.FontStyle = FontStyles.Normal;
                this.winParent.Close();
            }

            // * key
            if (e.Key == Key.Multiply)
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                    _currentContainer.ops.Add(concat.intersection);
                }
                else
                {
                    _currentContainer.ops.Add(concat.intersection);
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            // %
            if (e.Key == Key.D5 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                FastPop.IsOpen = true;
                _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Containing));
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            // $
            if (e.Key == Key.D4 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                FastPop.IsOpen = true;
                _currentContainer.content.Add(new ConsoleStringSelElement("$", ConsoleElementModifier.Selection));
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            // #
            if (e.Key == Key.D3 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Children));
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            // @
            if (e.Key == Key.D2 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Visible));
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            // !
            if (e.Key == Key.D1 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                FastPop.IsOpen = true;
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity != null)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Not));
                }
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            if (e.Key == Key.OemPlus)
            {
                FastPop.IsOpen = true;
                _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                shortcutMode = true;
                autoCompleteBox.FontStyle = FontStyles.Italic;
                autoCompleteBox.Text = "";
                e.Handled = true;
            }
            if (e.Key == Key.Back)
            {
                if (consoleMode && autoCompleteBox.Text == "")
                {
                    if (_currentContainer.content.Count > 0 || _currentContainer.ops.Count > 0)
                    {
                        if (_currentContainer.content.Count > _currentContainer.ops.Count)
                        {
                            _currentContainer.content.RemoveAt(_currentContainer.content.Count - 1);
                        }
                        else
                        {
                            _currentContainer.ops.RemoveAt(_currentContainer.ops.Count - 1);
                        }
                    }
                    else
                    {
                        if (_currentContainer.parent != null)
                        {
                            _currentContainer = _currentContainer.parent;
                        }
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                winParent.Close();
            }
            fastTip.Text = _consoleRoot.getCorrespondingStr();
        }

    }
}
