﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Serialization;
using System.Windows.Forms;
using WinLib.Network;
using WinLib.WinAPI;
using System.Xml;
using System.Threading;
using System.Collections.Concurrent;

namespace Network_Manager
{
    public class Config
    {
        public GadgetConfig Gadget = new GadgetConfig();
        public List<InterfaceProfile> InterfaceProfiles = new List<InterfaceProfile>();
        public SavedRouteList SavedRoutes = new SavedRouteList();
        public LoadBalancerConfig LoadBalancer = new LoadBalancerConfig();
        [XmlIgnore]
        public ConcurrentBag<InterfaceDataUsage> DataUsage = new ConcurrentBag<InterfaceDataUsage>();
        [XmlElement("DataUsage")]
        public InterfaceDataUsage[] DataUsageXml { get { return DataUsage.ToArray(); } set {
                DataUsage = value != null ? new ConcurrentBag<InterfaceDataUsage>(value) : new ConcurrentBag<InterfaceDataUsage>(); } }
        [field: NonSerialized]
        object saveLock = new object();
        
        public class GadgetConfig
        {
            public bool Debug = false;
            public bool CheckForUpdates = true;
            public bool AutoDetectInterfaces = false;
            public bool AlwaysOnTop = false;
            public int MaxVerticalSlots = 3;
            public int MaxHorizontalSlots = 1;
            public int GraphTimeSpan = 60;
            public Point Location = new Point(Screen.GetWorkingArea(Cursor.Position).Left + Screen.GetWorkingArea(Cursor.Position).Width / 2 - 100
                , Screen.GetWorkingArea(Cursor.Position).Top + Screen.GetWorkingArea(Cursor.Position).Height / 2 - 200);
            public List<Guid> HiddenInterfaces = new List<Guid>();
        }

        public class LoadBalancerConfig
        {
            public List<Guid> ExcludedInterfacesForTap = new List<Guid>();
            public List<Guid> ExcludedInterfacesForWindows = new List<Guid>();
            public List<NetworkInterface.IPHostAddress> IPv4LocalAddresses = new List<NetworkInterface.IPHostAddress>();
            public List<NetworkInterface.IPGatewayAddress> IPv4GatewayAddresses = new List<NetworkInterface.IPGatewayAddress>();
            public List<string> IPv4DnsAddresses = new List<string>();
            public bool ShowTrayTipsWarnings = true;
        }

        public class InterfaceProfile
        {
            public string Name;
            public List<NetworkInterface.IPHostAddress> IPv4LocalAddresses = new List<NetworkInterface.IPHostAddress>();
            public List<NetworkInterface.IPGatewayAddress> IPv4GatewayAddresses = new List<NetworkInterface.IPGatewayAddress>();
            public List<string> IPv4DnsAddresses = new List<string>();
            public NetworkInterface.Dhcp DhcpEnabled = NetworkInterface.Dhcp.Unchanged;
            public NetworkInterface.Netbios NetbiosEnabled = NetworkInterface.Netbios.Unchanged;
            public int IPv4Mtu = -1;
            public List<NetworkInterface.IPHostAddress> IPv6LocalAddresses = new List<NetworkInterface.IPHostAddress>();
            public List<NetworkInterface.IPGatewayAddress> IPv6GatewayAddresses = new List<NetworkInterface.IPGatewayAddress>();
            public List<string> IPv6DnsAddresses = new List<string>();
            public NetworkInterface.RouterDiscovery IPv6RouterDiscoveryEnabled = NetworkInterface.RouterDiscovery.Unchanged;
            public int IPv6Mtu = -1;
            public int InterfaceMetric = -1;
            public InterfaceDataUsage InterfaceDataUsage = null;
            public LoadingMode LoadMode = LoadingMode.ReplaceModified;

            public enum LoadingMode
            {
                /// <summary>
                /// Clear up modified IP sections before loading (default)
                /// </summary>
                ReplaceModified,
                /// <summary>
                /// Clear up all IP sections before load
                /// </summary>
                ReplaceAll,
                /// <summary>
                /// Append IP configurations to the existing ones instead of replacing them
                /// </summary>
                Append
            }
        }

        public class SavedRouteList
        {
            [field: NonSerialized]
            public event EventHandler<EventArgs> NodesChanged;

            public List<SavedRouteNode> Nodes = new List<SavedRouteNode>();

            /// <summary>
            /// Populates TreeView nodes with saved routes nodes
            /// </summary>
            /// <param name="treeView"></param>
            /// <param name="selectionPath"></param>
            public void Populate(TreeView treeView, List<string> selectionPath = null)
            {
                TreeNode selectedNode = treeView.SelectedNode;
                if (selectionPath == null && selectedNode != null)
                {
                    selectionPath = new List<string>();
                    TreeNode tvNode = selectedNode;
                    while (tvNode.Parent != null)
                    {
                        selectionPath.Insert(0, tvNode.Parent.Text);
                        tvNode = tvNode.Parent;
                    }
                    selectionPath.Add(selectedNode.Text);
                }
                treeView.ImageList = new ImageList();
                treeView.ImageList.Images.Add(WinLib.WinAPI.Shell32.ExtractIcon("shell32.dll", 3, false)); // group
                treeView.ImageList.Images.Add(WinLib.WinAPI.Shell32.ExtractIcon("shell32.dll", 72, false)); // item
                treeView.Nodes.Clear();
                PopulateNode(treeView.Nodes, this.Nodes);
                treeView.Nodes[0].Expand();
                selectedNode = treeView.Nodes[0];
                if (selectionPath != null)
                {
                    foreach (string node in selectionPath.Skip(1))
                        if (selectedNode.Nodes.ContainsKey(node))
                            selectedNode = selectedNode.Nodes.Find(node, false).First();
                        else
                            break;
                }
                treeView.SelectedNode = selectedNode;
            }

            private void PopulateNode(TreeNodeCollection tvNodes, List<SavedRouteNode> nodes)
            {
                nodes = nodes.OrderBy(i => i.Name).ToList();
                foreach (SavedRouteNode node in nodes)
                {
                    if (node is SavedRouteItem)
                    {
                        TreeNode itemNode = tvNodes.Add(node.Name, node.Name);
                        itemNode.ImageIndex = 1;
                        itemNode.SelectedImageIndex = 1;
                    }
                    else if (node is SavedRouteGroup)
                    {
                        TreeNode groupNode = tvNodes.Add(node.Name, node.Name);
                        groupNode.ImageIndex = 0;
                        groupNode.SelectedImageIndex = 0;
                        PopulateNode(groupNode.Nodes, ((SavedRouteGroup)node).Nodes);
                    }
                }
            }

            /// <summary>
            /// For rename = true, if node already exists and source is a group it renames it without changing its subnodes,
            /// and if it's an item it replaces it.<para/>
            /// Returns:<para/>
            /// 0: success<para/>
            /// 1: invlid destination<para/>
            /// 2: name already exists
            /// </summary>
            /// <param name="treeView"></param>
            /// <param name="name"></param>
            /// <param name="rename">Rename group/edit item</param>
            /// <returns></returns>
            public int AddNode(TreeView treeView, SavedRouteNode node, bool rename = false)
            {
                TreeNode selectedNode = treeView.SelectedNode;
                if (selectedNode == null)
                    return 1;
                // get TV selection path
                List<string> path = new List<string>();
                TreeNode tvNode = selectedNode;
                while (tvNode.Parent != null)
                {
                    path.Insert(0, tvNode.Parent.Text);
                    tvNode = tvNode.Parent;
                }
                if (selectedNode.ImageIndex == 0)
                    path.Add(selectedNode.Text);
                // find equivalent node in config
                List<SavedRouteNode> destination = this.Nodes;
                foreach (string pathNode in path.Take(path.Count - Convert.ToInt32(rename) + selectedNode.ImageIndex))
                    destination = ((SavedRouteGroup)destination.Find((i) => i.Name == pathNode && i is SavedRouteGroup)).Nodes;
                // find if name already used at destination
                if (destination.Find((i) => i.Name == node.Name && i.GetType() == node.GetType()) != null &&
                    (!rename || node.Name != selectedNode.Text))
                    return 2;
                if (rename)
                {
                    if (selectedNode.ImageIndex == 0)
                    {
                        if (destination.Find((i) => i.Name == selectedNode.Text && i.GetType() == node.GetType()) != null)
                        {
                            SavedRouteGroup group = (SavedRouteGroup)destination.Find((i) => i.Name == selectedNode.Text && i.GetType() == node.GetType());
                            group.Name = node.Name;
                            group.AutoLoadOnStartup = ((SavedRouteGroup)node).AutoLoadOnStartup;
                        }
                        path.Remove(path.Last());
                    }
                    else
                    {
                        destination.Remove(destination.Find((i) => i.Name == selectedNode.Text && i.GetType() == node.GetType()));
                        destination.Add(node);
                    }
                }
                else
                    destination.Add(node);
                path.Add(node.Name);
                // update TV
                Global.Config.Save();
                Global.Config.SavedRoutes.Populate(treeView, path);
                // raise event
                if (NodesChanged != null)
                    NodesChanged(this, new EventArgs());
                return 0;
            }

            /// <summary>
            /// Get the SavedRouteNode based on Tree View selection
            /// </summary>
            /// <param name="treeView"></param>
            /// <param name="getParent"></param>
            /// <returns></returns>
            public SavedRouteNode GetSelectedNode(TreeView treeView, bool getParent = false, TreeNode selectedNode = null)
            {
                if (selectedNode == null)
                    selectedNode = treeView.SelectedNode;
                if (selectedNode == null)
                    return null;
                // get TV selection path
                List<string> path = new List<string>();
                TreeNode tvNode = selectedNode;
                while (tvNode.Parent != null)
                {
                    path.Insert(0, tvNode.Parent.Text);
                    tvNode = tvNode.Parent;
                }
                if (!getParent)
                    path.Add(selectedNode.Text);
                // find equivalent node in config
                List<SavedRouteNode> target = this.Nodes;
                foreach (string pathNode in path.Take(path.Count - 1))
                    target = ((SavedRouteGroup)target.Find((i) => i.Name == pathNode && i is SavedRouteGroup)).Nodes;
                SavedRouteNode getNode = null;
                foreach (SavedRouteNode node in target)
                    if (node.Name == path.Last() &&
                        ((getParent ? selectedNode.Parent.ImageIndex == 0 : selectedNode.ImageIndex == 0) ? node.GetType() == typeof(SavedRouteGroup) : node.GetType() == typeof(SavedRouteItem)))
                    {
                        getNode = node;
                        break;
                    }
                return getNode;
            }

            /// <summary>
            /// Find the first SavedRouteItem based on destination+prefix+gateway+interface and return a Tree View with the equivalent node selected,
            /// that can be passed to the other functions
            /// </summary>
            /// <param name="route"></param>
            /// <returns></returns>
            public TreeView Find(SavedRouteItem route)
            {
                List<string> path = FindPath(route);
                TreeView treeView = new TreeView();
                Populate(treeView, path);
                return treeView;
            }

            private List<string> FindPath(SavedRouteItem route, SavedRouteGroup rootNode = null)
            {
                if (rootNode == null)
                    rootNode = (SavedRouteGroup)Nodes[0];
                foreach (SavedRouteNode node in rootNode.Nodes)
                {
                    if (node is SavedRouteItem)
                    {
                        SavedRouteItem item = (SavedRouteItem)node;
                        if (item.IPVersion == route.IPVersion)
                            if (item.Destination == route.Destination)
                                if (item.Prefix == route.Prefix)
                                    if (item.Gateway == route.Gateway)
                                        if (item.InterfaceGuid == route.InterfaceGuid)
                                            return new List<string>(new string[] { rootNode.Name, item.Name });
                    }
                    else
                    {
                        SavedRouteGroup group = (SavedRouteGroup)node;
                        List<string> relativePath = FindPath(route, group);
                        if (relativePath != null)
                            return new List<string>(new string[] { rootNode.Name }).Concat(relativePath).ToList();
                    }
                }
                return null;
            }

            public int DeleteNode(TreeView treeView)
            {
                TreeNode selectedNode = treeView.SelectedNode;
                if (selectedNode == null)
                    return 1;
                // get TV selection path
                List<string> path = new List<string>();
                TreeNode tvNode = selectedNode;
                while (tvNode.Parent != null)
                {
                    path.Insert(0, tvNode.Parent.Text);
                    tvNode = tvNode.Parent;
                }
                path.Add(selectedNode.Text);
                // find equivalent node in config
                List<SavedRouteNode> target = this.Nodes;
                foreach (string pathNode in path.Take(path.Count - 1))
                    target = ((SavedRouteGroup)target.Find((i) => i.Name == pathNode && i is SavedRouteGroup)).Nodes;
                SavedRouteNode deleteNode = null;
                foreach (SavedRouteNode node in target)
                    if (node.Name == path.Last() && (selectedNode.ImageIndex == 0 ? node.GetType() == typeof(SavedRouteGroup) : node.GetType() == typeof(SavedRouteItem)))
                    {
                        deleteNode = node;
                        break;
                    }
                if (deleteNode != null)
                    target.Remove(deleteNode);
                // update TV
                path.RemoveAt(path.Count - 1);
                Global.Config.Save();
                Global.Config.SavedRoutes.Populate(treeView, path);
                // raise event
                if (NodesChanged != null)
                    NodesChanged(this, new EventArgs());
                return 0;
            }

            public int MoveNode(TreeView treeView, TreeNode source, TreeNode destination)
            {
                if (source == null || destination == null)
                    return 1;
                // get source TV path
                List<string> sourcePath = new List<string>();
                TreeNode tvNode = source;
                while (tvNode.Parent != null)
                {
                    sourcePath.Insert(0, tvNode.Parent.Text);
                    tvNode = tvNode.Parent;
                }
                sourcePath.Add(source.Text);
                // get destination TV path
                List<string> destiantionPath = new List<string>();
                tvNode = destination;
                while (tvNode.Parent != null)
                {
                    destiantionPath.Insert(0, tvNode.Parent.Text);
                    tvNode = tvNode.Parent;
                }
                if (destination.ImageIndex == 0)
                    destiantionPath.Add(destination.Text);
                // find equivalent nodes in config
                List<SavedRouteNode> sourceNodes = this.Nodes;
                foreach (string pathNode in sourcePath.Take(sourcePath.Count - 1))
                    sourceNodes = ((SavedRouteGroup)sourceNodes.Find(i => i.Name == pathNode && i is SavedRouteGroup)).Nodes;
                SavedRouteNode sourceNode;
                if (source.ImageIndex == 0)
                    sourceNode = sourceNodes.Find(i => i.Name == sourcePath.Last() && i is SavedRouteGroup);
                else
                    sourceNode = sourceNodes.Find(i => i.Name == sourcePath.Last() && i is SavedRouteItem);
                List<SavedRouteNode> destinationNodes = this.Nodes;
                foreach (string pathNode in destiantionPath)
                    destinationNodes = ((SavedRouteGroup)destinationNodes.Find((i) => i.Name == pathNode && i is SavedRouteGroup)).Nodes;
                // find if name already used at destination
                if (destinationNodes.Find(i => i.Name == sourceNode.Name && i.GetType() == sourceNode.GetType()) != null)
                    return 2;
                // move node
                destinationNodes.Add(sourceNode);
                sourceNodes.Remove(sourceNode);
                // update TV
                destiantionPath.Add(sourceNode.Name);
                Global.Config.Save();
                Global.Config.SavedRoutes.Populate(treeView, destiantionPath);
                // raise event
                if (NodesChanged != null)
                    NodesChanged(this, new EventArgs());
                return 0;
            }

            /// <summary>
            /// Recursively receives the routes from a node into a list
            /// </summary>
            /// <param name="node"></param>
            /// <param name="autoLoadOnly"></param>
            /// <returns></returns>
            public List<SavedRouteItem> GetRoutes(SavedRouteNode node, bool autoLoadOnly = false)
            {
                List<SavedRouteItem> routes = new List<SavedRouteItem>();
                if (node is SavedRouteGroup)
                    foreach (SavedRouteNode subNode in ((SavedRouteGroup)node).Nodes)
                    {
                        if (((SavedRouteGroup)node).AutoLoadOnStartup || !autoLoadOnly || subNode is SavedRouteGroup)
                            routes.AddRange(GetRoutes(subNode, autoLoadOnly));
                    }
                else if (node is SavedRouteItem)
                    routes.Add((SavedRouteItem)node);
                return routes;
            }
        }

        [XmlInclude(typeof(SavedRouteItem))]
        [XmlInclude(typeof(SavedRouteGroup))]
        public class SavedRouteNode 
        {
            public string Name;
            
            public enum ImageIndex
            {
                Group,
                Item
            }
        }

        public class SavedRouteItem : SavedRouteNode
        {
            public string Destination;
            public string Prefix;
            public string Gateway;
            public Guid InterfaceGuid;
            public ushort Metric;
            public int IPVersion;

            /// <summary>
            /// Load in FIB
            /// </summary>
            /// <param name="nic">Override saved egress interface</param>
            /// <param name="gateway">Override saved gateway</param>
            /// <param name="metric">Override saved metric</param>
            public void Load(NetworkInterface nic = null, string gateway = null, int metric = -1)
            {
                if (nic == null)
                {
                    if (Global.NetworkInterfaces.ContainsKey(this.InterfaceGuid))
                        nic = Global.NetworkInterfaces[this.InterfaceGuid];
                    else if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) > -1 &&
                        NetworkInterface.Loopback.Guid == this.InterfaceGuid)
                        nic = NetworkInterface.Loopback;
                }
                if (gateway == null)
                    gateway = Gateway;
                if (metric < 0)
                    metric = Metric;
                // if on-link set type to direct for XP
                Iphlpapi.MIB_IPFORWARD_TYPE type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_INDIRECT;
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                {
                    if (this.IPVersion == 4)
                    {
                        if (nic.IPv4Address.Where((i) => i.Address == gateway).Count() > 0)
                            type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                    }
                    else
                    {
                        if (nic.IPv6Address.All.Where((i) => i.Address == gateway).Count() > 0)
                            type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                    }
                    if (gateway == "0.0.0.0" || gateway == "::")
                        type = Iphlpapi.MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT;
                }
                // correction for Vista->XP transitioned saved route
                if (Environment.OSVersion.Version.CompareTo(new Version("6.0")) < 0)
                {
                    if (IPAddress.TryParse(gateway, out IPAddress ipAddress))
                        if (IPAddress.Parse(gateway).GetAddressBytes().Max() == 0)
                            if (this.IPVersion == 4)
                                gateway = nic.IPv4Address.First().Address;
                            else
                                gateway = nic.IPv6Address.All.First().Address;
                    if (metric == 0)
                        metric = 1;
                }
                Iphlpapi.DeleteRoute(this.Destination, this.Prefix, gateway, nic.Index.ToString());
                Iphlpapi.AddRoute(this.Destination, this.Prefix, gateway, nic.Index.ToString(), metric.ToString(), type);
            }
        }

        public class SavedRouteGroup : SavedRouteNode
        {
            public List<SavedRouteNode> Nodes = new List<SavedRouteNode>();
            public bool AutoLoadOnStartup = false;
        }

        public class InterfaceDataUsage
        {
            public Guid InterfaceGuid;
            public bool Track = false;
            public long PreviousSessionsReceivedBytes = 0;
            public long PreviousSessionsSentBytes = 0;
            public long CurrentSessionReceivedBytes = 0;
            public long CurrentSessionSentBytes = 0;
            public ResetIntervals ResetInterval = ResetIntervals.Never;
            [XmlIgnore]
            public TimeSpan MomentOfTheDayForReset = TimeSpan.MinValue;
            [XmlElement("MomentOfTheDayForReset")]
            public long MomentOfTheDayForResetXml { get { return MomentOfTheDayForReset.Ticks; } set { MomentOfTheDayForReset = new TimeSpan(value); } }
            public DayOfWeek DayOfTheWeekForReset = DayOfWeek.Sunday;
            public int DayOfTheMonthForReset = 1;
            public DateTime LastReset = DateTime.MinValue;
            [XmlIgnore]
            public DateTime LastSignificantFlow = DateTime.MinValue;
            [XmlIgnore]
            public bool SignificantFlow = false;


            public enum ResetIntervals
            {
                Never,
                Daily,
                Weekly,
                Monthly
            }

            public void Update(NetworkInterface networkInterface)
            {
                if (Track)
                {
                    // new connection
                    if (CurrentSessionReceivedBytes > networkInterface.IPv4BytesReceived || CurrentSessionSentBytes > networkInterface.IPv4BytesSent)
                    {
                        Interlocked.Add(ref PreviousSessionsReceivedBytes, CurrentSessionReceivedBytes);
                        Interlocked.Add(ref PreviousSessionsSentBytes, CurrentSessionSentBytes);
                        CurrentSessionReceivedBytes = networkInterface.IPv4BytesReceived;
                        CurrentSessionSentBytes = networkInterface.IPv4BytesSent;
                        SignificantFlow = true;
                        LastSignificantFlow = DateTime.Now;
                    }
                    if ((networkInterface.IPv4BytesReceived > CurrentSessionReceivedBytes + 1024 * 1024 || networkInterface.IPv4BytesSent > CurrentSessionSentBytes + 1024 * 1024) && DateTime.Now > LastSignificantFlow.AddMinutes(1) ||
                        DateTime.Now > LastSignificantFlow.AddMinutes(10))
                    {
                        CurrentSessionReceivedBytes = networkInterface.IPv4BytesReceived;
                        CurrentSessionSentBytes = networkInterface.IPv4BytesSent;
                        SignificantFlow = true;
                        LastSignificantFlow = DateTime.Now;
                    }
                    // reset
                    if (ResetInterval == ResetIntervals.Daily && (DateTime.Now.TimeOfDay >= MomentOfTheDayForReset && LastReset < DateTime.Now.Subtract(DateTime.Now.TimeOfDay).Add(MomentOfTheDayForReset) || LastReset <= DateTime.Now.Subtract(DateTime.Now.TimeOfDay).Add(MomentOfTheDayForReset).AddDays(-1)) ||
                        ResetInterval == ResetIntervals.Weekly && (DateTime.Now.DayOfWeek >= DayOfTheWeekForReset && LastReset < DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).AddDays((int)DayOfTheWeekForReset).Subtract(DateTime.Now.TimeOfDay) || LastReset <= DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).AddDays((int)DayOfTheWeekForReset).AddDays(-7).Subtract(DateTime.Now.TimeOfDay)) ||
                        ResetInterval == ResetIntervals.Monthly && (DateTime.Now.Day >= DayOfTheMonthForReset && LastReset < DateTime.Now.AddDays(-DateTime.Now.Day).AddDays(DayOfTheMonthForReset).Subtract(DateTime.Now.TimeOfDay) || LastReset <= DateTime.Now.AddDays(-DateTime.Now.Day).AddDays(DayOfTheMonthForReset).AddMonths(-1).Subtract(DateTime.Now.TimeOfDay)))
                    {
                        SignificantFlow = true;
                        Interlocked.Exchange(ref PreviousSessionsReceivedBytes, 0);
                        Interlocked.Exchange(ref PreviousSessionsSentBytes, 0);
                        LastReset = DateTime.Now;
                    }
                }
            }
        }

        public void Save()
        {
            lock (saveLock)
            {
                XmlSerializer writer = new XmlSerializer(typeof(Config));
                StreamWriter file = new StreamWriter("Network_Manager.xml.tmp");
                XmlWriter xmlWriter = XmlWriter.Create(file);
                lock (DataUsage)
                    writer.Serialize(file, this);
                file.Close();
                if (File.Exists("Network_Manager.xml"))
                    File.Delete("Network_Manager.xml");
                File.Move("Network_Manager.xml.tmp", "Network_Manager.xml");
            }
        }

        public static void Load()
        {
            Config currentConfig;
            if (!File.Exists("Network_Manager.xml") && File.Exists("Network_Manager.xml.tmp"))
                File.Move("Network_Manager.xml.tmp", "Network_Manager.xml");
            if (File.Exists("Network_Manager.xml"))
            {
                try
                {
                    XmlSerializer reader = new XmlSerializer(typeof(Config));
                    using (StreamReader file = new StreamReader("Network_Manager.xml"))
                    {
                        XmlReader xmlReader = XmlReader.Create(file);
                        currentConfig = (Config)reader.Deserialize(xmlReader);
                        //Config defaultConfig = new Config();
                        //currentConfig = (Config)CheckIfNull(typeof(Config), currentConfig, defaultConfig); // not needed, XML does a nice job
                    }
                }
                catch
                {
                    DialogResult result = MessageBox.Show("Configuration file was corrupted.\n\nDo you want to reset it to default and lose all configurations?", "Config File Corrupted", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.ServiceNotification);
                    if (result == DialogResult.No)
                        Global.Exit();
                    currentConfig = new Config();
                    File.Delete("Network_Manager.xml");
                }
            }
            else
                currentConfig = new Config();
            // load defaults if null
            if (currentConfig.SavedRoutes.Nodes.Count == 0)
            {
                currentConfig.SavedRoutes.Nodes.Add(new SavedRouteGroup());
                currentConfig.SavedRoutes.Nodes[0].Name = "<Root Group>";
            }
            if (currentConfig.InterfaceProfiles.Count == 0)
            {
                InterfaceProfile profile = new InterfaceProfile();
                profile.Name = "Google DNS";
                profile.IPv4DnsAddresses.Add("8.8.8.8");
                profile.IPv4DnsAddresses.Add("8.8.4.4");
                profile.IPv6DnsAddresses.Add("2001:4860:4860::8888");
                profile.IPv6DnsAddresses.Add("2001:4860:4860::8844");
                currentConfig.InterfaceProfiles.Add(profile);
                profile = new InterfaceProfile();
                profile.Name = "Comodo Secure DNS";
                profile.IPv4DnsAddresses.Add("8.26.56.26");
                profile.IPv4DnsAddresses.Add("8.20.247.20");
                currentConfig.InterfaceProfiles.Add(profile);
                profile = new InterfaceProfile();
                profile.Name = "OpenDNS";
                profile.IPv4DnsAddresses.Add("208.67.222.222");
                profile.IPv4DnsAddresses.Add("208.67.220.220");
                profile.IPv6DnsAddresses.Add("2620:0:ccc::2");
                profile.IPv6DnsAddresses.Add("2620:0:ccd::2");
                currentConfig.InterfaceProfiles.Add(profile);
            }
            if (currentConfig.LoadBalancer.IPv4LocalAddresses.Count == 0)
                currentConfig.LoadBalancer.IPv4LocalAddresses.Add(new NetworkInterface.IPHostAddress("192.168.200.2", "255.255.255.0"));
            if (currentConfig.LoadBalancer.IPv4GatewayAddresses.Count == 0)
                currentConfig.LoadBalancer.IPv4GatewayAddresses.Add(new NetworkInterface.IPGatewayAddress("192.168.200.1", 1));
            if (currentConfig.LoadBalancer.IPv4DnsAddresses.Count == 0)
                currentConfig.LoadBalancer.IPv4DnsAddresses.Add("8.8.8.8");
            // legacy upgrade support
            if (File.Exists("Network_Manager.ini"))
            {
                // recover Gadget settings
                string gadgetX = Kernel32.IniReadValue("Gadget", "GadgetX", "Network_Manager.ini");
                string gadgetY = Kernel32.IniReadValue("Gadget", "GadgetY", "Network_Manager.ini");
                string alwaysOnTop = Kernel32.IniReadValue("Gadget", "AlwaysOnTop", "Network_Manager.ini");
                string checkForUpdates = Kernel32.IniReadValue("Gadget", "CheckForUpdates", "Network_Manager.ini");
                string maxVerticalSlots = Kernel32.IniReadValue("Gadget", "MaxVerticalSlots", "Network_Manager.ini");
                string maxHorizontalSlots = Kernel32.IniReadValue("Gadget", "MaxHorizontalSlots", "Network_Manager.ini");
                if (Regex.IsMatch(gadgetX, @"^\d+$") &&
                    Regex.IsMatch(gadgetY, @"^\d+$"))
                    currentConfig.Gadget.Location = new Point(int.Parse(gadgetX), int.Parse(gadgetY));
                if (Regex.IsMatch(alwaysOnTop, @"^(0|1)$"))
                    currentConfig.Gadget.AlwaysOnTop = alwaysOnTop == "1";
                if (Regex.IsMatch(checkForUpdates, @"^(0|1)$"))
                    currentConfig.Gadget.CheckForUpdates = checkForUpdates == "1";
                if (Regex.IsMatch(maxVerticalSlots, @"^\d$"))
                    currentConfig.Gadget.MaxVerticalSlots = int.Parse(maxVerticalSlots);
                if (Regex.IsMatch(maxHorizontalSlots, @"^\d$"))
                    currentConfig.Gadget.MaxHorizontalSlots = int.Parse(maxHorizontalSlots);
                // recover routes
                string routeNames = Kernel32.IniReadValue("Routes", "RouteNames", "Network_Manager.ini");
                if (routeNames.Contains('#'))
                {
                    SavedRouteGroup group = new SavedRouteGroup();
                    group.Name = "Legacy recovered routes";
                    
                    foreach (string routeName in routeNames.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] sRoute = Kernel32.IniReadValue("Routes", routeName, "Network_Manager.ini").Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                        SavedRouteItem route = new SavedRouteItem();
                        route.Name = routeName;
                        try
                        {
                            route.Destination = sRoute[0];
                            route.Prefix = sRoute[1];
                            route.Gateway = sRoute[2];
                            route.InterfaceGuid = Guid.Empty;
                            route.Metric = ushort.Parse(sRoute[4]);
                            route.IPVersion = 4;
                            group.Nodes.Add(route);
                        }
                        catch { }
                    }
                    ((SavedRouteGroup)currentConfig.SavedRoutes.Nodes[0]).Nodes.Add(group);
                }
                // recover interface profiles
                // complete profiles
                string completeProfileNames = Kernel32.IniReadValue("ConfigInterfaceProfiles", "CompleteProfileNames", "Network_Manager.ini");
                foreach (string profileName in completeProfileNames.Split(new [] {'#'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (currentConfig.InterfaceProfiles.Any(i => i.Name == profileName))
                        continue;
                    string[] sProfile = Kernel32.IniReadValue("ConfigInterfaceProfiles", profileName, "Network_Manager.ini").Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    InterfaceProfile profile = new InterfaceProfile();
                    profile.Name = profileName;
                    try
                    {
                        profile.IPv4LocalAddresses.Add(new NetworkInterface.IPHostAddress(sProfile[0], sProfile[1]));
                        profile.IPv4GatewayAddresses.Add(new NetworkInterface.IPGatewayAddress(sProfile[2], ushort.Parse(sProfile[3])));
                        profile.IPv4DnsAddresses.Add(sProfile[5]);
                        //profile.DhcpEnabled = (NetworkInterface.Dhcp)(int.Parse(sProfile[4]) + int.Parse(sProfile[6]));
                        profile.InterfaceMetric = int.Parse(sProfile[7]);
                        profile.NetbiosEnabled = (NetworkInterface.Netbios)(int.Parse(sProfile[7]) == 1 ? 1 : 2);
                        currentConfig.InterfaceProfiles.Add(profile);
                    }
                    catch { }
                }
                // IP profiles
                string ipProfileNames = Kernel32.IniReadValue("ConfigInterfaceProfiles", "IpProfileNames", "Network_Manager.ini");
                foreach (string profileName in ipProfileNames.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (currentConfig.InterfaceProfiles.Any(i => i.Name == profileName))
                        continue;
                    string[] sProfile = Kernel32.IniReadValue("ConfigInterfaceProfiles", profileName, "Network_Manager.ini").Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    InterfaceProfile profile = new InterfaceProfile();
                    profile.Name = profileName;
                    try
                    {
                        profile.IPv4LocalAddresses.Add(new NetworkInterface.IPHostAddress(sProfile[0], sProfile[1]));
                        profile.IPv4GatewayAddresses.Add(new NetworkInterface.IPGatewayAddress(sProfile[2], ushort.Parse(sProfile[3])));
                        currentConfig.InterfaceProfiles.Add(profile);
                    }
                    catch { }
                }
                // DNS profiles
                string dnsProfileNames = Kernel32.IniReadValue("ConfigInterfaceProfiles", "DnsProfileNames", "Network_Manager.ini");
                foreach (string profileName in dnsProfileNames.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (currentConfig.InterfaceProfiles.Any(i => i.Name == profileName))
                        continue;
                    string[] sProfile = Kernel32.IniReadValue("ConfigInterfaceProfiles", profileName, "Network_Manager.ini").Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    InterfaceProfile profile = new InterfaceProfile();
                    profile.Name = profileName;
                    try
                    {
                        profile.IPv4DnsAddresses.Add(sProfile[0]);
                        currentConfig.InterfaceProfiles.Add(profile);
                    }
                    catch { }
                }
                // Settings profiles
                string settingsProfileNames = Kernel32.IniReadValue("ConfigInterfaceProfiles", "SettingsProfileNames", "Network_Manager.ini");
                foreach (string profileName in settingsProfileNames.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (currentConfig.InterfaceProfiles.Any(i => i.Name == profileName))
                        continue;
                    string[] sProfile = Kernel32.IniReadValue("ConfigInterfaceProfiles", profileName, "Network_Manager.ini").Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    InterfaceProfile profile = new InterfaceProfile();
                    profile.Name = profileName;
                    try
                    {
                        profile.InterfaceMetric = int.Parse(sProfile[7]);
                        profile.NetbiosEnabled = (NetworkInterface.Netbios)(int.Parse(sProfile[7]) == 1 ? 2 : 0);
                        currentConfig.InterfaceProfiles.Add(profile);
                    }
                    catch { }
                }
                try
                {
                    File.Move("Network_Manager.ini", "Network_Manager.old.ini");
                }
                catch { }
            }
            Global.Config = currentConfig;
        }
    }
}
