using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;


namespace com.csutil.webgl {

    /// <summary> This is a script that manages the connection from unity to the share functions 
    /// of a browser, when the project is being compiled to WebGL </summary>
    public class ShareManager : MonoBehaviour {


        /// <summary> Import JSLib functions:
        /// 
        /// This is where we reference the javaScript funtctions we have written
        /// into our .jslib file. If you have included cscore as a module, the
        /// javaScript code will automatically be served to the browser.
        ///
        /// More info at:
        /// https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
        /// </summary>

        #region jsFunctionImport
        [DllImport("__Internal")]
        private static extern bool sharejs(string title, string text, string url, string file, string fileName);

        [DllImport("__Internal")]
        private static extern bool downloadFilejs(string file, string fileName);

        [DllImport("__Internal")]
        private static extern bool canSharejs();
        #endregion



        /// <summary>
        /// This function uses the share() function to share a text, url or file in given in a byte array.
        /// It converts the file to base 64 so i can be send to the browser without problems.
        /// </summary>
        /// <param name="title"> Sharing title </param>
        /// <param name="text"> Sharing messaage </param>
        /// <param name="url"> Shared url </param>
        /// <param name="file"> Shared file in byte array</param>
        /// <param name="fileName"> Shared file name</param>
        /// <returns>
        /// true if the share process was activated
        /// </returns>
        public bool share(string title, string text, string url = "", byte[] file = null, string fileName = "") {
            return share(title, text, url, Convert.ToBase64String(file), fileName);
        }

        /// <summary>
        /// This function uses the JSLib File to share a text, url or file in given in base 64.
        /// </summary>
        /// <param name="title"> Sharing title </param>
        /// <param name="text"> Sharing messaage </param>
        /// <param name="url"> Shared url </param>
        /// <param name="file"> Shared file in base 64</param>
        /// <param name="fileName"> Shared file name</param>
        /// <returns>
        /// true if the share process was activated
        /// </returns>
        public bool share(string title, string text, string url, string base64FileString, string fileName) {
            return sharejs(title, text, url, base64FileString, fileName);
        }


        /// <summary>
        /// This function uses the JSLib File to download a file on the browser.
        /// </summary>
        /// <param name="base64FileString"> Shared file in base 64</param>
        /// <param name="fileName"> Shared file name</param>
        /// <returns>
        /// true if the download was activated
        /// </returns>
        public bool downloadFile(string base64FileString, string fileName) {
            return downloadFilejs(base64FileString, fileName);
        }

        /// <summary>
        /// This function uses the downloadFile() function to download a file on the browser.
        /// It convets the file to base 64.
        /// </summary>
        /// <param name="file"> Shared file in byte array</param>
        /// <param name="fileName"> Shared file name</param>
        /// <returns>
        /// true if the download was activated
        /// </returns>
        public bool downloadFile(byte[] file = null, string fileName = "") {
            return downloadFile(Convert.ToBase64String(file), fileName);
        }

        /// <summary>
        /// This functions uses the JSLib File to check if the browser can share.
        /// </summary>
        /// <returns>
        /// True if the browser can share
        /// </returns>
        public bool canShare() {
            return canSharejs();
        }
    }
}