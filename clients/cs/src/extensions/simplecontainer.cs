/* Copyright (c) 2006 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections;
using System.Text;
using System.Xml;
using Google.GData.Client;
using System.Globalization;

namespace Google.GData.Extensions {

    /// <summary>
    /// base class to implement extensions holding extensions
    /// TODO: at one point think about using this as the base for atom:base
    /// as there is some utility overlap between the 2 of them
    /// </summary>
    public class SimpleContainer : ExtensionBase
    {
        private ArrayList extensions;
        private ArrayList extensionFactories;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">the xml name</param>
        /// <param name="prefix">the xml prefix</param>
        /// <param name="ns">the xml namespace</param>
        protected SimpleContainer(string name, string prefix, string ns) : base(name, prefix, ns)
        {
        }

       
       
        #region overloaded for persistence

        //////////////////////////////////////////////////////////////////////
        /// <summary>the list of extensions for this container
        /// the elements in that list MUST implement IExtensionElementFactory 
        /// and IExtensionElement</summary> 
        /// <returns> </returns>
        //////////////////////////////////////////////////////////////////////
        public ArrayList ExtensionElements
        {
            get 
            {
                if (this.extensions == null)
                {
                    this.extensions = new ArrayList();
                }
                return this.extensions;
            }
            set {this.extensions = value;}
        }


        /// <summary>
        /// Finds a specific ExtensionElement based on it's local name
        /// and it's namespace. If namespace is NULL, the first one where
        /// the localname matches is found. If there are extensionelements that do 
        /// not implment ExtensionElementFactory, they will not be taken into account
        /// </summary>
        /// <param name="localName">the xml local name of the element to find</param>
        /// <param name="ns">the namespace of the elementToPersist</param>
        /// <returns>Object</returns>
        public Object FindExtension(string localName, string ns) 
        {
            return Utilities.FindExtension(this.extensions, localName, ns);
        }

        /// <summary>
        /// all extension elements that match a namespace/localname
        /// given will be removed and the new one will be inserted
        /// </summary> 
        /// <param name="localName">the local name to find</param>
        /// <param name="ns">the namespace to match, if null, ns is ignored</param>
        /// <param name="obj">the new element to put in</param>
        public void ReplaceExtension(string localName, string ns, Object obj)
        {

            DeleteExtensions(localName, ns);
            this.ExtensionElements.Add(obj);
        }

        /// <summary>
        /// Finds all ExtensionElement based on it's local name
        /// and it's namespace. If namespace is NULL, allwhere
        /// the localname matches is found. If there are extensionelements that do 
        /// not implment ExtensionElementFactory, they will not be taken into account
        /// Primary use of this is to find XML nodes
        /// </summary>
        /// <param name="localName">the xml local name of the element to find</param>
        /// <param name="ns">the namespace of the elementToPersist</param>
        /// <param name="arr">the array to fill</param>
        /// <returns>none</returns>
        public ArrayList FindExtensions(string localName, string ns) 
        {
            return Utilities.FindExtensions(this.extensions, 
                                            localName, ns, new ArrayList());

        }

        /// <summary>
        /// Delete's all Extensions from the Extension list that match
        /// a localName and a Namespace. 
        /// </summary>
        /// <param name="localName">the local name to find</param>
        /// <param name="ns">the namespace to match, if null, ns is ignored</param>
        /// <returns>int - the number of deleted extensions</returns>
        public int DeleteExtensions(string localName, string ns) 
        {
            // Find them first
            ArrayList arr = FindExtensions(localName, ns);
            foreach (object ob in arr)
            {
                this.extensions.Remove(ob);
            }
            return arr.Count;
        }


        //////////////////////////////////////////////////////////////////////
        /// <summary>the list of extensions for this container
        /// the elements in that list MUST implement IExtensionElementFactory 
        /// and IExtensionElement</summary> 
        /// <returns> </returns>
        //////////////////////////////////////////////////////////////////////
        public ArrayList ExtensionFactories
        {
            get 
            {
                if (this.extensionFactories == null)
                {
                    this.extensionFactories = new ArrayList();
                }
                return this.extensionFactories;
            }
            set {this.extensionFactories = value;}
        }

        // end of accessor public ArrayList Extensions


        //////////////////////////////////////////////////////////////////////
        /// <summary>Parses an xml node to create a Who object.</summary> 
        /// <param name="node">the node to work on, can be NULL</param>
        /// <returns>the created SimpleElement object</returns>
        //////////////////////////////////////////////////////////////////////
        public override IExtensionElement CreateInstance(XmlNode node) 
        {
            Tracing.TraceCall("for: " + XmlName);

            if (node != null)
            {
                object localname = node.LocalName;
                if (localname.Equals(this.XmlName) == false ||
                    node.NamespaceURI.Equals(this.XmlNameSpace) == false)
                {
                    return null;
                }
            }

            SimpleContainer sc = null;
            
            // create a new container
            sc = this.MemberwiseClone() as SimpleContainer;
      
            if (node != null && node.HasChildNodes)
            {
                XmlNode childNode = node.FirstChild;
                while (childNode != null && childNode is XmlElement)
                {
                    foreach (IExtensionElementFactory f in this.ExtensionFactories)
                    {
                        if (String.Compare(childNode.NamespaceURI, f.XmlNameSpace) == 0)
                        {
                            if (String.Compare(childNode.LocalName, f.XmlName) == 0)
                            {
                                Tracing.TraceMsg("Added extension to SimpleContainer for: " + f.XmlName);
                                sc.ExtensionElements.Add(f.CreateInstance(childNode));
                                break;
                            }
                        }
                    }
                    childNode = childNode.NextSibling;
                }
            }
            return sc;
        }

         
        /// <summary>
        /// Persistence method for the EnumConstruct object
        /// </summary>
        /// <param name="writer">the xmlwriter to write into</param>
        public override void Save(XmlWriter writer)
        {
            writer.WriteStartElement(XmlPrefix, XmlName, XmlNameSpace);
            if (this.extensions != null)
            {
                foreach (IExtensionElement e in this.ExtensionElements)
                {
                    e.Save(writer);
                }
            }
            writer.WriteEndElement();
        }
        #endregion
    }
}  