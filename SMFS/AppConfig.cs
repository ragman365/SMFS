using System;
using System.Xml;  
using System.Configuration;
using System.Collections;
using System.Reflection;  
using System.Diagnostics ;
using System.Windows.Forms;
using System.IO;

namespace GeneralLib
{
	public enum   ConfigFileType
	{
		WebConfig ,
		AppConfig
	}

	public class AppConfig : System.Configuration.AppSettingsReader
	{	
		public string 	docName = String.Empty;
		private  XmlNode node=null;
		private int _configType;

		public   int ConfigType
		{
			get
			{
				return _configType;
			}
			set
			{
				_configType=value;
			}
		}

		public void LoadDoc()
		{
			try
			{
				XmlDocument cfgDoc = new XmlDocument();
				loadConfigDoc(cfgDoc);
			}
			catch{}
		}

		public bool SetValue(string key, string value)
		{
			XmlDocument cfgDoc = new XmlDocument();	
			loadConfigDoc(cfgDoc);
			// retrieve the appSettings node 
			node =  cfgDoc.SelectSingleNode("//appSettings");
			
			if( node == null )
			{
				throw new System.InvalidOperationException( "appSettings section not found"); 
			}
			
			try
			{
				// XPath select setting "add" element that contains this key 	  
				XmlElement addElem= (XmlElement)node.SelectSingleNode("//add[@key='" +key +"']") ;
				if (addElem!=null)
				{
					addElem.SetAttribute("value",value);			 
				}
					// not found, so we need to add the element, key and value
				else 
				{
					XmlElement entry = cfgDoc.CreateElement("add");
					entry.SetAttribute("key",key);
					entry.SetAttribute("value",value); 
					node.AppendChild(entry);				
				}
				//save it
				saveConfigDoc(cfgDoc,Application.StartupPath + @"\" + docName);
				return true;
			}
			catch 
			{
				return false;
			}
		}
	
		private void saveConfigDoc(XmlDocument cfgDoc,string cfgDocPath)
		{	
			try
			{				
				XmlTextWriter writer = new XmlTextWriter( cfgDocPath , null );
				writer.Formatting = Formatting.Indented;					
				cfgDoc.WriteTo( writer );					
				writer.Flush();
				writer.Close(); 	 
				return;
			}
			catch
			{
				throw;
			}
		}
 
		public bool removeElement ( string elementKey)
		{
			try
			{
				XmlDocument cfgDoc = new XmlDocument();	
				loadConfigDoc(cfgDoc);
				// retrieve the appSettings node 
				node =  cfgDoc.SelectSingleNode("//appSettings");			
				if( node == null )
				{
					throw new System.InvalidOperationException( "appSettings section not found"); 
				}			
				// XPath select setting "add" element that contains this key to remove	  
				node.RemoveChild(	node.SelectSingleNode("//add[@key='" +elementKey +"']") );
		 
				saveConfigDoc(cfgDoc,docName);
				return true;
			}
			catch
			{
				return false;
			}
		}


		private XmlDocument loadConfigDoc( XmlDocument cfgDoc )
		{ 
			// load the config file 
			if(  Convert.ToInt32(ConfigType)==Convert.ToInt32(ConfigFileType.AppConfig))
			{
			 
				docName= ((Assembly.GetEntryAssembly()).GetName()).Name;
				docName +=   ".exe.config";
			}
			else
			{
//				docName=System.Web.HttpContext.Current.Server.MapPath("web.config");
			}

			cfgDoc.Load(Application.StartupPath + @"\" + docName ); 
			return cfgDoc;
		}

	}
}