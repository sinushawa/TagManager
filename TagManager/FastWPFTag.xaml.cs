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
		private TagCenter tagCenter;
        private List<TagNode> nodesList;
        private List<string> branchNames;
        private TagNode projectEntity;
		private bool consoleMode = false;
		private ConsoleContainerElement _consoleRoot;
		private ConsoleContainerElement _currentContainer;
		public Window winParent;
        public List<string> baseNames = new List<string>() { "Root", "Project" };

		public FastWPFTag()
		{
			this.InitializeComponent();
		}
		public void CreateAutoCompleteSource(TagCenter _tagCenter)
		{
			tagCenter = _tagCenter;
            projectEntity = tagCenter.fastPan.Root.GetNodeList().First(x => x.Name == "Project");
            nodesList = projectEntity.Children.GetNodeList().ToList();
            branchNames= new List<string>();
            foreach (TagNode _entity in nodesList)
            {
                string _branchName = _entity.GetNodeBranchName(tagCenter.fastPan.delimiter, baseNames);
                branchNames.Add(_branchName);
            }
            this.FastBox.ItemsSource = branchNames;
			_consoleRoot = new ConsoleContainerElement();
			_currentContainer = _consoleRoot;
		}
		private TagNode RetrieveEntityFromTag(string _tag)
		{
            if (branchNames.Contains(_tag))
            {
                int index = branchNames.IndexOf(_tag);
                TagNode result = nodesList[index];
                return result;
            }
            else
            {
                return null;
            }
		}
		private List<TagNode> RetrieveEntitiesContainsTag(string _tag)
		{
            List<TagNode> result = new List<TagNode>();
            for (int i = 0; i < branchNames.Count; i++ )
            {
                if (branchNames[i].Contains(_tag))
                {
                    result.Add(nodesList[i]);
                }
            }
            return result;
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
				TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
				if (entity != null)
				{
					_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, entity.Nodes.ToList()));
				}
                autoCompleteBox.Text = "";
				this._currentContainer = this._currentContainer.parent;
				e.Handled = true;
			}
			if (e.Key == Key.Add)
			{
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
				if (!this.consoleMode)
				{
					if (entity == null)
					{
                        entity = new TagNode(autoCompleteBox.Text, MaxPluginUtilities.Selection.ToListHandles());
                        projectEntity.Children.Add(entity);
					}
					else
					{
                        entity.Nodes.AddRange(MaxPluginUtilities.Selection.ToListHandles());
					}
					if (tagCenter.fastPan.autoRename)
					{
                        TagMethods.RenameUsingStructure(tagCenter.fastPan.Root, tagCenter.fastPan.delimiter, baseNames);
					}
				}
				else
				{
					if (entity != null)
					{
                        this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, MaxPluginUtilities.Selection.ToListHandles()));
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
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
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
                    if (tagCenter.fastPan.autoRename)
					{
                        TagMethods.RenameUsingStructure(tagCenter.fastPan.Root, tagCenter.fastPan.delimiter, baseNames);
					}
					this.winParent.Close();
				}
				else
				{
                    if (entity != null)
					{
						this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, entity.Nodes.ToList()));
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
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
                if (entity == null)
				{
                    entity = new TagNode(autoCompleteBox.Text, MaxPluginUtilities.Selection.ToListHandles());
                    projectEntity.Children.Add(entity);
				}
				else
				{
					entity.Nodes.AddRange(MaxPluginUtilities.Selection.ToListHandles());
				}
                if (tagCenter.fastPan.autoRename)
				{
                    TagMethods.RenameUsingStructure(tagCenter.fastPan.Root, tagCenter.fastPan.delimiter, baseNames);
				}
				this.winParent.Close();
			}
			if (e.Key == Key.Return)
			{
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
				if (!this.consoleMode)
				{
					List<TagNode> entitiesToSelect = new List<TagNode>();
                    entitiesToSelect.Add(entity);
					if (tagCenter.fastPan.childrenAutoSelect)
					{
                        entitiesToSelect.AddRange(entity.Children.GetNodeList());
					}
					if (entity != null)
					{
                        List<uint> objectsToSelect = new List<uint>();
                        foreach (TagNode _entity in entitiesToSelect)
						{
                            objectsToSelect.AddRange(_entity.Nodes);
						}
                        objectsToSelect = objectsToSelect.Distinct().ToList();
                        MaxPluginUtilities.setSelection(objectsToSelect, tagCenter.fastPan.newSelection);
					}
				}
				else
				{
					if (entity != null)
					{
						this._currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, entity.Nodes.ToList()));
					}
                    MaxPluginUtilities.setSelection(_currentContainer.getCorrespondingSel(), tagCenter.fastPan.newSelection);
				}
				this.winParent.Close();
			}
			if (e.Key == Key.Multiply)
			{
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
				if (this.consoleMode)
				{
					if (entity != null)
					{
						_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text, entity.Nodes.ToList()));
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
                    List<uint> objects = RetrieveEntitiesContainsTag(autoCompleteBox.Text).SelectMany((TagNode x) => x.Nodes).ToList<uint>();
					_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text + "%", objects));
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
			if (e.Key == Key.D4 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
			{
				if (this.consoleMode)
				{
					this._currentContainer.content.Add(new ConsoleStringSelElement("$", MaxPluginUtilities.Selection.ToListHandles()));
				}
				autoCompleteBox.Text="";
				e.Handled = true;
			}
			if (e.Key == Key.D3 && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
			{
                TagNode entity = RetrieveEntityFromTag(autoCompleteBox.Text);
				if (this.consoleMode)
				{
					if (entity != null)
					{
						List<uint> objects2 = entity.GetNodeList().SelectMany((TagNode x) => x.Nodes).ToList<uint>();
						_currentContainer.content.Add(new ConsoleStringSelElement(autoCompleteBox.Text + "#", objects2));
					}
				}
				autoCompleteBox.Text="";
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
