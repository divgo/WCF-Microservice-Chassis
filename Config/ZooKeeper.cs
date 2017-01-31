using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ZooKeeperNet;

namespace Microservice.Configuration
{

	public class CollectionChangeEventArgs : EventArgs
	{
		public string path { get; set; }
		public CollectionChangeEventArgs(string newPath)
		{
			this.path = newPath;
		}
	}

	public class ZooWatcher : IWatcher
	{

		public delegate void ZooChangeEvent(object sender, CollectionChangeEventArgs e);
		public event ZooChangeEvent WatcherEvent;

		private void log(string message)
		{
			Logging.LogWriter.INFO(message);
			//using (StreamWriter sw = File.AppendText("C:\\Credible\\dev\\CredibleCore\\legacy_build\\log2.txt"))
			//{
			//	sw.WriteLine(System.DateTime.Now.ToString() + "\t" + message);
			//}
		}
		protected virtual void OnChange(CollectionChangeEventArgs e)
		{
			log("zooWatcher.onChange Called");
			log(e.path);

			if (WatcherEvent != null)
			{
				log("zooWatcher.onChange WatcherEvent is not null");
				WatcherEvent(this, e);
			}
			else
			{
				log("zooWatcher.onChange WatcherEvent IS null");
			}
		}

		public void Process(WatchedEvent wEvent) {

			string path = wEvent.Path.ToString();
			log("zooWatcher.Process Called");
			log("path: " + path);

			if (wEvent.GetType().Equals(EventType.None))
			{
				// We are are being told that the state of the connection has changed
				switch (wEvent.State) {
					case KeeperState.SyncConnected:
						// In this particular example we don't need to do anything
						// here - watches are automatically re-registered with 
						// server and any watches triggered while the client was 
						// disconnected will be delivered (in order of course)
						break;
					case KeeperState.Expired:
						// It's all over
						// dead = true;
						// listener.closing(KeeperException.Code.SessionExpired);
						break;
				}
			}
			else
			{
				if (!String.IsNullOrEmpty(path))
				{
					OnChange(new CollectionChangeEventArgs(path));
				}
			}
			KeeperState state = wEvent.State;
			EventType eType = wEvent.Type;
			string ewPath = wEvent.Wrapper.Path;
			int ewState = wEvent.Wrapper.State;
			int ewType = wEvent.Wrapper.Type;
		}
	}
	public class Zoo
	{

		private ZooKeeper zk;
		private String zk_hosts;
		private bool doWatch = false;
		private ZooKeeperNet.ClientConnection connSignal;

		public Zoo(String conn)
		{
			this.zk_hosts = conn;
			this.connect(this.zk_hosts);
		}
		public Zoo(String conn, ZooWatcher watcher)
		{
			this.zk_hosts = conn;
			this.doWatch = true;
			this.connect(this.zk_hosts, watcher);
		}

		public void connect(String host) // ZooKeeper 
		{
			zk = new ZooKeeper(host, System.TimeSpan.FromSeconds(3000), new ZooWatcher());
		}
		public void connect(String host, ZooWatcher watcher) // ZooKeeper 
		{
			zk = new ZooKeeper(host, System.TimeSpan.FromSeconds(3000), watcher);
		}
		public void close()
		{
			if (zk.State == ZooKeeper.States.CONNECTED)
			{
				zk.Dispose();
			}
			
			zk = null;
		}
		public void createNode(String path, byte[] data)
		{
			if (!String.IsNullOrEmpty(path))
			{
				zk.Create(path, data, ZooKeeperNet.Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
			};
		}
		public void updateNode(String path, byte[] data)
		{
			if (!String.IsNullOrEmpty(path))
			{
				zk.SetData(path, data, zk.Exists(path, true).Version);
			}
		}
		public void deleteNode(String path)
		{
			if (!String.IsNullOrEmpty(path))
			{
				zk.Delete(path, zk.Exists(path, true).Version);
			}
		}

		// GETTERS

		public byte[] getNodeDataRaw(String path, bool watch)
		{
			byte[] result = null;
			if (!String.IsNullOrEmpty(path))
			{
				try
				{
					return zk.GetData(path, this.doWatch, zk.Exists(path, false));
				}
				catch (ZooKeeperNet.KeeperException ke)
				{
					this.close();
					return null;
				}
			} else
			{
				return null;
			}
		}
		public string getNodeDataString(String path, bool watch)
		{
			byte[] result = null;

			if (!String.IsNullOrEmpty(path))
			{
				try
				{
					result = zk.GetData(path, this.doWatch, zk.Exists(path, false));
				}
				catch (ZooKeeperNet.KeeperException ke)
				{
					this.close();
				}
			}
			
			if (result!=null && result.Length > 0)
			{
				return System.Text.Encoding.UTF8.GetString(result);
			}
			else
			{
				return null;
			}
		}
		public IEnumerable<String> getNodeChildren(String path, bool watch)
		{
			if (zk != null)
			{
				if (!String.IsNullOrEmpty(path))
				{
					return zk.GetChildren(path, this.doWatch, zk.Exists(path, false));
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
