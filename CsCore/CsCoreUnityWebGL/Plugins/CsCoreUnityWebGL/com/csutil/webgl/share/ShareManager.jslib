/**
 * This is how javascript functions are implemented in a way that
 * makes them callable from within the C# code. More info at:
 * https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
 */
mergeInto(LibraryManager.library, {
  /**
   * This function uses the JavaScript Share API to share a URL
   * @param titlePtr Pointer to the string location from the C# title
   * @param textPtr  Pointer to the string location from the C# text
   * @param urlPtr  Pointer to the string location from the C# url
   * @param base64FilePtr Pointer to the base 64 file string location from the C# file
   * @param fileNamePtr Pointer to the string location from the C# filename
   * @returns true if the data sharing process has been started. 
   */
  sharejs: function (titlePtr, textPtr, urlPtr, base64FilePtr, fileNamePtr) {

    if (!canSharejs()) {
      return false
    }
    //Convert pointer to Strings and only ads attribute to share data if its not empty (default)
    //#region 
    shareData = {
      title: UTF8ToString(titlePtr)
    }

    text = UTF8ToString(textPtr)
    if (text != "") {
      shareData.text = text
    }

    url = UTF8ToString(urlPtr)
    if (url != "") {
      shareData.url = url
    }

    base64string = UTF8ToString(base64FilePtr)
    fileName = UTF8ToString(fileNamePtr)

    //#endregion

    // Decodes the base 64 string and make a new File, which is added to the shareDate.
    if (fileName != "" && fileName != "") {
      decodedString = atob(base64string);

      const byteNumbers = new Array(decodedString.length);
      for (let i = 0; i < decodedString.length; i++) {
      byteNumbers[i] = decodedString.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers); 
      
      blob = new Blob([byteArray])
      
      shareData.files = [new File([byteArray], fileName, {type: "text/plain"})];
      
      if(navigator.canShare(shareData)){
        navigator.share(shareData)
        return true
      } else return false
      
      }
      
    else {
      navigator.share(shareData)
      return true
    }

  },

  /**
   * 
   * @param base64FilePtr Pointer to the base 64 file string location from the C# file
   * @param fileNamePtr Pointer to the string location from the C# filename
   * @returns true if the download was successful 
   */
  downloadFilejs: function (base64FilePtr, fileNamePtr) {

    //Converts pointer to string
    base64string = UTF8ToString(base64FilePtr)
    fileName = UTF8ToString(fileNamePtr)

    // Decodes the base 64 string and make a new File. After that it will be downloaded.
    if (fileName != "" && fileName != "") {
      decodedString = atob(base64string);

      const byteNumbers = new Array(decodedString.length);
      for (let i = 0; i < decodedString.length; i++) {
      byteNumbers[i] = decodedString.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers); 
      
      blob = new Blob([byteArray])
      
      file = new File([byteArray], fileName, {type: "text/plain"});
    

      const link = document.createElement('fileElement')
      const url = URL.createObjectURL(shareData.files)

      link.href = url
      link.download = file.name
      document.body.appendChild(link)
      link.click()
      return true 
    } else return false

  },
 
  /**
   * This functions checks if the browser has the canShare functions
   * @returns true if the canShare function exists in the browser
   */
  canSharejs: function () {
    if (navigator.canShare) {
      return true;
    } else return false;
  }
});
