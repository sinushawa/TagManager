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
            branchNames= new List<string>();
            foreach (TagNode _entity in nodesList)
            {
                string _branchName = _entity.GetNodeBranchName(TagGlobals.delimiter, TagGlobals.baseNames);
                branchNames.Add(_branchName);
            }
            this.FastBox.ItemsSource = branchNames;
			_consoleRoot = new ConsoleContainerElement();
			_currentContainer = _consoleRoot;
		}
		
		private void FastBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			AutoCompleteBox autoCompleteBox = (AutoCompleteBox)sender;
			if (e.Key == Key.Oem4)
			{
				this.consoleMode = true;
				this.FastPop.IsOpen = true;
				ConsoleContainerElement consoleContainerElement = new ConsoleContainerElement(this._currentContainer);
				this._currentContainer.content.Add(consoleContainerElement);
				this._currentContainer = consoleContainerElement;
				e.Handled = true;
			}
			if (e.Key == Key.Oem6)
			{
				TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (entity != null)
				{
					_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
				}
                autoCompleteBox.Text = "";
				this._currentContainer = this._currentContainer.parent;
				e.Handled = true;
			}
			if (e.Key == Key.Add)
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (!this.consoleMode)
				{
					if (entity == null)
					{
                        entity = new TagNode(autoCompleteBox.Text, MaxPluginUtilities.Selection.ToListHandles());
                        projectEntity.Children.Add(entity);
					}
					else
					{
                        //either of the possibilities work
                        //TagMethods.ApplyEntities(new List<TagNode>() { entity }, MaxPluginUtilities.Selection.ToListHandles());
                        entity.Nodes.AddRange(MaxPluginUtilities.Selection.ToListHandles(), true);
					}
					if (TagGlobals.autoRename)
					{
                        TagMethods.RenameUsingStructure(TagGlobals.root);
					}
				}
				else
				{
					if (entity != null)
					{
                        this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
						this._currentContainer.ops.Add(concat.addition);
					}
					else
					{
						this._currentContainer.ops.Add(concat.addition);
					}
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
			if (e.Key == Key.Subtract)
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (!this.consoleMode)
				{
                    if (entity != null)
					{
						using (IEnumerator<uint> enumerator = MaxPluginUtilities.Selection.ToListHandles().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								uint _obj = enumerator.Current;
								if (entity.Nodes.Any((uint x) => x == _obj))
								{
                                    entity.Nodes.Remove(_obj);
								}
							}
						}
					}
                    if (TagGlobals.autoRename)
					{
                        TagMethods.RenameUsingStructure(TagGlobals.root);
					}
					this.winParent.Close();
				}
				else
				{
                    if (entity != null && autoCompleteBox.Text != "")
					{
						this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
						this._currentContainer.ops.Add(concat.substraction);
					}
					else
					{
						this._currentContainer.ops.Add(concat.substraction);
					}
					autoCompleteBox.Text="";
					e.Handled = true;
				}
			}
			if (e.Key == Key.Oem2)
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity == null)
				{
                    entity = TagHelperMethods.GetLonguestMatchingTag(autoCompleteBox.Text, true);
				}
				entity.Nodes.AddRange(MaxPluginUtilities.Selection.ToListHandles());
                if (TagGlobals.autoRename)
				{
                    TagMethods.RenameUsingStructure(TagGlobals.root);
				}
				this.winParent.Close();
			}
			if (e.Key == Key.Return)
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (!this.consoleMode)
				{
					List<TagNode> entitiesToSelect = new List<TagNode>();
                    entitiesToSelect.Add(entity);
					if (TagGlobals.childrenAutoSelect)
					{
                        entitiesToSelect.AddRange(entity.Children.ToList().GetNodeList());
					}
					if (entity != null)
					{
                        List<uint> objectsToSelect = new List<uint>();
                        foreach (TagNode _entity in entitiesToSelect)
						{
                            objectsToSelect.AddRange(_entity.Nodes);
						}
                        objectsToSelect = objectsToSelect.Distinct().ToList();
                        MaxPluginUtilities.SetSelection(objectsToSelect);
					}
				}
				else
				{
                    if (shortcutMode)
                    {
                        entity = TagHelperMethods.GetLonguestMatchingTag(autoCompleteBox.Text, true);
                        TagNode _shortcut = new TagNode(autoCompleteBox.Text, _consoleRoot);
                        entity.Children.Add(_shortcut);
                    }
					if (entity != null)
					{
						this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
					}
                    MaxPluginUtilities.SetSelection(_currentContainer.getCorrespondingSel());
				}
				this.winParent.Close();
			}
			if (e.Key == Key.Multiply)
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (this.consoleMode)
				{
					if (entity != null)
					{
						_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
						_currentContainer.ops.Add(concat.intersection);
					}
					else
					{
						this._currentContainer.ops.Add(concat.intersection);
					}
					autoCompleteBox.Text="";
					e.Handled = true;
				}
			}
			if (e.Key == Key.D5 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
			{
				if (this.consoleMode)
				{
					_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Containing));
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
			if (e.Key == Key.D4 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
			{
				if (this.consoleMode)
				{
					this._currentContainer.content.Add(new ConsoleStringSelElement("$", ConsoleElementModifier.Selection));
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
			if (e.Key == Key.D3 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
			{
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(autoCompleteBox.Text);
				if (this.consoleMode)
				{
					if (entity != null)
					{
						_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.Children));
					}
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
            if (e.Key == Key.OemPlus )
            {
                if (consoleMode)
                {
                    _currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, ConsoleElementModifier.None));
                    shortcutMode = true;
                }
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
				this.winParent.Close();
			}
			fastTip.Text = _consoleRoot.getCorrespondingStr();
		}
		
	}
}
