using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hsl.Xrm.Sdk;
using System.Xml.Linq;

namespace Hsl.Xrm.Sdk.Plugin
{
    /// <summary>
    /// A base class for managing settings for a plugin.
    /// </summary>
    /// <remarks>
    /// This class will attempt to read the attributes of a "settings" node as the settings for
    /// the configuration.  It is also possible to read a custom object from the configuration using the
    /// GetObject method.  See the GetObject method comments for more information.
    /// </remarks>
    public class PluginConfigurationManager
    {
        #region Constructor & Destructor Implementations

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PluginConfigurationManager()
        {
            _UnsecureConfigText = string.Empty;
            _UnsecureConfig = new ParameterCollection();

            _SecureConfigText = string.Empty;
            _SecureConfig = new ParameterCollection();

            _AllConfig = new ParameterCollection();
        }

        /// <summary>
        /// Initializes an instance of the object by initializing with the config and secureconfig parameters
        /// that are passed to the plugin upon instantiation.
        /// </summary>
        /// <param name="unsecureConfig">The config passed to the plugin.</param>
        /// <param name="secureConfig">The secureconfig passed to the plugin.</param>
        public PluginConfigurationManager(string unsecureConfig, string secureConfig)
            : this()
        {
            _UnsecureConfigText = unsecureConfig;
            _SecureConfigText = secureConfig;

            InitializeSettings(_UnsecureConfig, _UnsecureConfigText);
            InitializeSettings(_SecureConfig, _SecureConfigText);
        }

        #endregion

        #region Property Implementations

        /// <summary>
        /// Gets the application settings from the app.config/web.config file.
        /// </summary>
        /// <returns></returns>
        public NameValueCollection AppSettings()
        {
            return ConfigurationManager.AppSettings;
        }

        /// <summary>
        /// All of the parameters from both the unsecure and secure configuations.  The secure configuration parameters will override any unsecure parameters values 
        /// that have the same name.
        /// </summary>
        public ParameterCollection AllConfig
        {
            get
            {
                return _AllConfig;
            }
        }
        private ParameterCollection _AllConfig;

        /// <summary>
        /// The sections that were loaded from the "secureconfig" for the plugin.
        /// </summary>
        public ParameterCollection SecureConfig
        {
            get
            {
                return _SecureConfig;
            }
        }
        private ParameterCollection _SecureConfig;

        /// <summary>
        /// Gets the secure configuration as raw text.
        /// </summary>
        /// <returns>The raw text contained in the secure configuration.</returns>
        public string SecureConfigText
        {
            get
            {
                return _SecureConfigText;
            }
        }
        private string _SecureConfigText;

        /// <summary>
        /// The sections that were loaded from the "config" for the plugin.
        /// </summary>
        public ParameterCollection UnsecureConfig
        {
            get
            {
                return _UnsecureConfig;
            }
        }
        private ParameterCollection _UnsecureConfig;

        /// <summary>
        /// Gets the unsecure configuration as raw text.
        /// </summary>
        /// <returns>The raw text contained in the unsecure configuration.</returns>
        public string UnsecureConfigText
        {
            get
            {
                return _UnsecureConfigText;
            }
        }
        private string _UnsecureConfigText;

        #endregion

        #region Public Method Implementations

        /// <summary>
        /// Initializes the config (unsecure) configuration.
        /// </summary>
        /// <param name="config">A string containing the configuration data.</param>
        public void InitializeConfig(string config)
        {
            _UnsecureConfigText = config.TrimIfNotNull();
            _UnsecureConfig = new ParameterCollection();
            InitializeSettings(_UnsecureConfig, _UnsecureConfigText);
        }

        /// <summary>
        /// Initializes the secure config configuration.
        /// </summary>
        /// <param name="secureConfig">A string containing the secure configuration data.</param>
        public void InitializeSecureConfig(string secureConfig)
        {
            _SecureConfigText = secureConfig.TrimIfNotNull();
            _SecureConfig = new ParameterCollection();
            InitializeSettings(_SecureConfig, _SecureConfigText);
        }

        /// <summary>
        /// Retrieves an object from the secure configuration at the specified XPATH using a <b>XmlSerializer</b> object.
        /// </summary>
        /// <remarks>
        /// The object being retrieved must be serializable using the XmlSerializer object otherwise an exception will be thrown.
        /// </remarks>
        /// <param name="xpath">The path of the object's location in the XML.</param>
        /// <param name="objectType">The type to be deserialized from the configuration XML.</param>
        /// <returns>An object of the specified type if it could be deserialized from the configuration XML.</returns>
        public object GetSecureConfigObject(string xpath, Type objectType)
        {
            return GetObject(xpath, objectType, _SecureConfigText);
        }

        /// <summary>
        /// Retrieves an object from the unsecure configuration at the specified XPATH using a <b>XmlSerializer</b> object.
        /// </summary>
        /// <remarks>
        /// The object being retrieved must be serializable using the XmlSerializer object otherwise an exception will be thrown.
        /// </remarks>
        /// <param name="xpath">The path of the object's location in the XML.</param>
        /// <param name="objectType">The type to be deserialized from the configuration XML.</param>
        /// <returns>An object of the specified type if it could be deserialized from the configuration XML.</returns>
        public object GetUnsecureConfigObject(string xpath, Type objectType)
        {
            return GetObject(xpath, objectType, _UnsecureConfigText);
        }

        #endregion

        #region Private Method Imlementations

        /// <summary>
        /// Retrieves an object from the configuration XML at the specified XPATH using a <b>XmlSerializer</b> object.
        /// </summary>
        /// <remarks>
        /// The object being retrieved must be serializable using the XmlSerializer object otherwise an exception will be thrown.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the <b>xpath</b> or the <b>objectType</b> arguments are <b>null</b>.</exception>
        /// <exception cref="ArgumentException">If the <b>xpath</b> argument is an empty string.</exception>
        /// <exception cref="ApplicationException">If the object could not be deserialized using an <b>XmlSerializer</b>.</exception>
        /// <param name="xpath">
        /// The path of the object's location in the XML.  The node returned by the XPATH expression will be passed
        /// to the XmlSerializer object as the XmlRootAttribute.  This means the properties of the class should
        /// be encoded as child elements of this node.
        /// </param>
        /// <param name="objectType">The type to be deserialized from the configuration XML.</param>
        /// <param name="configXml">The configuration XML containing the object to be deserialized.</param>
        /// <returns>An object of the specified type if it could be deserialized from the configuration XML.</returns>
        /// <example>
        /// For an object name MyObject with property MyProp1 and MyProp2 where the XPATH  is 
        /// /*/MyCustomObject the XML would appear as follows.
        /// <code>
        /// <root>
        ///     <MyCustomObject>
        ///         <MyProp1>myvalue1</MyProp1>
        ///         <MyProp2>2</MyProp2>
        ///     </MyCustomObject>
        /// </root>
        /// </code>
        /// </example>
        private object GetObject(string xpath, Type objectType, string configXml)
        {
            StringReader sin = null;
            object data = null;

            // If the xml was not supplied then get out of here
            if (string.IsNullOrWhiteSpace(configXml))
            {
                return null;
            }

            #region Argument Validation

            if (xpath == null)
            {
                throw new ArgumentNullException("xpath");
            }

            if (string.IsNullOrWhiteSpace(xpath))
            {
                throw new ArgumentException("The [" + xpath + "] argument cannot be an empty string.");
            }

            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            #endregion

            try
            {
                // Load the configuration text into a XML document and find the node specified by the xpath
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(configXml);
                XmlNode xnode = xdoc.SelectSingleNode(xpath);

                // Load the XML specifying the serialized object into a stream and deserialize it
                if (xnode != null)
                {
                    sin = new StringReader(xnode.OuterXml);
                    XmlRootAttribute root = new XmlRootAttribute(xnode.LocalName);
                    XmlSerializer serializer = new XmlSerializer(objectType, root);
                    data = serializer.Deserialize(sin);
                }
            }
            catch (Exception ex)
            {
                // AAAAAHHHHHHHHHHHHHHHHHHH! #%$@! To throw or not to throw?
                throw new ApplicationException("Unable to retrieve the object of type [" + objectType.ToString() + "] at the specified XPATH of [" + xpath + "].", ex);
            }
            finally
            {
                // Clean up objects
                if (sin != null)
                {
                    sin.Close();
                }
            }

            return data;
        }

        /// <summary>
        /// Initialize the sections from the config that is specified in the config string.
        /// </summary>
        /// <param name="dic">The <b>Dictionary<string,string></b> to which the settings will be added.</param>
        /// <param name="config">The configuration as a string.</param>
        /// <remarks>
        /// If an item does not conform to the expected structure it will be ignored.
        /// </remarks>
        private void InitializeSettings(ParameterCollection pc, string config)
        {
            // Get out of here if nothing was specified in the config
            if (string.IsNullOrWhiteSpace(config) || !config.StartsWith("<"))
            {
                return;
            }

            var xml = XElement.Parse(config);
            var settings = xml.Elements("settings")
                .DefaultIfEmpty(new XElement("settings"))
                .First()
                .Attributes();


            foreach (var xatt in settings)
            {
                pc[xatt.Name.LocalName] = xatt.Value;
                _AllConfig[xatt.Name.LocalName] = xatt.Value;
            }
        }

        #endregion
    }
}
