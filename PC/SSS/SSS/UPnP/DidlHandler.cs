/*
 *	 Smart Streaming System
 *	 Assembly: SSS
 *	 File: DidlHandler.cs
 *	 Copyright (C) 2012-2015 - Ronch IT.	   	
 *
 *   This file is part of Smart Streaming System.
 */

namespace SSS.UPnP
{
    using System.Web;

    internal class DidlHandler
    {
        public static string GetContainer(string id, string parentID, string title, string upnpClass = "object.container", string restricted = "0")
        {
            string retVal =
            "<container id=\"" + id + "\" restricted=\"" + restricted + "\" parentID=\"" + parentID + "\">" +
            "<dc:title>" + HttpUtility.HtmlEncode(title) + "</dc:title>" +
            "<upnp:class>" + upnpClass + "</upnp:class>" +
            "</container>";
            return retVal;
        }

        public static string BeginDidl()
        {
            return "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">";
        }

        public static string EndDidl()
        {
            return "</DIDL-Lite>";
        }
    }
}
