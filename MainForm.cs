using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace FileWalk
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form, ContainerVisitor
	{
		private System.Windows.Forms.TreeView treeViewContainer;
		private System.Windows.Forms.TextBox textBoxDesc;
		private System.Windows.Forms.Splitter splitterLeft;
		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.MenuItem menuItemExit;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItemOpenFile;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ImageList imageListTree;
		private System.ComponentModel.IContainer components;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.treeViewContainer = new System.Windows.Forms.TreeView();
			this.imageListTree = new System.Windows.Forms.ImageList(this.components);
			this.textBoxDesc = new System.Windows.Forms.TextBox();
			this.splitterLeft = new System.Windows.Forms.Splitter();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuItemFile = new System.Windows.Forms.MenuItem();
			this.menuItemOpenFile = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItemExit = new System.Windows.Forms.MenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// treeViewContainer
			// 
			this.treeViewContainer.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeViewContainer.ImageList = this.imageListTree;
			this.treeViewContainer.Location = new System.Drawing.Point(0, 0);
			this.treeViewContainer.Name = "treeViewContainer";
			this.treeViewContainer.SelectedImageIndex = 1;
			this.treeViewContainer.Size = new System.Drawing.Size(248, 385);
			this.treeViewContainer.TabIndex = 0;
			this.treeViewContainer.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnSelectNode);
			// 
			// imageListTree
			// 
			this.imageListTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageListTree.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTree.ImageStream")));
			this.imageListTree.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// textBoxDesc
			// 
			this.textBoxDesc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxDesc.Location = new System.Drawing.Point(248, 0);
			this.textBoxDesc.Multiline = true;
			this.textBoxDesc.Name = "textBoxDesc";
			this.textBoxDesc.ReadOnly = true;
			this.textBoxDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxDesc.Size = new System.Drawing.Size(488, 385);
			this.textBoxDesc.TabIndex = 1;
			this.textBoxDesc.Text = "";
			// 
			// splitterLeft
			// 
			this.splitterLeft.Location = new System.Drawing.Point(248, 0);
			this.splitterLeft.Name = "splitterLeft";
			this.splitterLeft.Size = new System.Drawing.Size(3, 385);
			this.splitterLeft.TabIndex = 2;
			this.splitterLeft.TabStop = false;
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItemFile});
			// 
			// menuItemFile
			// 
			this.menuItemFile.Index = 0;
			this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemOpenFile,
																						 this.menuItem1,
																						 this.menuItemExit});
			this.menuItemFile.Text = "File";
			// 
			// menuItemOpenFile
			// 
			this.menuItemOpenFile.Index = 0;
			this.menuItemOpenFile.Text = "&Open File...";
			this.menuItemOpenFile.Click += new System.EventHandler(this.OnOpenFile);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 1;
			this.menuItem1.Text = "-";
			// 
			// menuItemExit
			// 
			this.menuItemExit.Index = 2;
			this.menuItemExit.Text = "E&xit";
			this.menuItemExit.Click += new System.EventHandler(this.OnExit);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(736, 385);
			this.Controls.Add(this.splitterLeft);
			this.Controls.Add(this.textBoxDesc);
			this.Controls.Add(this.treeViewContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "FileWalk";
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private ContainerFile _container;

		private void OnLoad(object sender, System.EventArgs e)
		{
		}

		private void OnExit(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void ResetTree(string fname)
		{
			textBoxDesc.Text = string.Empty;

			treeViewContainer.Nodes.Clear();
			TreeNode root = new TreeNode(fname);
			treeViewContainer.Nodes.Add(root);

			_currentNode = root;
		}

		#region ContainerVisitor Members

		private TreeNode _currentNode;

		public void BeginVisitNode(string name, string desc)
		{
			TreeNode node = new TreeNode(name);
			node.Tag = desc;
			_currentNode.Nodes.Add(node);

			_currentNode = node;
		}

		public void EndVisitNode()
		{
			_currentNode = _currentNode.Parent;
		}

		#endregion

		private void OnSelectNode(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node.Tag != null)
			{
				textBoxDesc.Text = (string) e.Node.Tag;
			}
		}

		private void OnOpenFile(object sender, System.EventArgs e)
		{
            openFileDialog.Filter = 
                "QuickTime/MP4 Files|*.mp4;*.3gp;*.mov|" +
                "AVI Files|*.avi;*.wav|" +
                "ESG Container|*.*|" +
                "ESG Access Descriptor|*.*";
			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				// according to its file extension
				switch (Path.GetExtension(openFileDialog.FileName).ToLower())
				{
					case ".mp4":
					case ".3gp":
					case ".mov":

						_container = new QuickTimeFile(openFileDialog.FileName);
						break;

					case ".avi":
					case ".wav":

						_container = new AviFile(openFileDialog.FileName);
						break;

                    default:
                        switch (openFileDialog.FilterIndex)
                        {
                            case 1:
                                _container = new QuickTimeFile(openFileDialog.FileName);
                                break;
                            case 2:
                                _container = new AviFile(openFileDialog.FileName);
                                break;
                            case 3:
                                _container = new EsgContainerFile(openFileDialog.FileName, EsgFileType.Container);
                                break;
                            case 4:
                                _container = new EsgContainerFile(openFileDialog.FileName, EsgFileType.AccessDescriptor);
                                break;
                        }
                        break;
				}

				if (_container != null)
				{
					ResetTree(openFileDialog.FileName);

					try
					{
						_container.Walk(this);
                        treeViewContainer.ExpandAll();
                        treeViewContainer.Nodes[0].EnsureVisible();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
					}
				}
			}		
		}
	}
}
