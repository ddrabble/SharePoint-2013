'use strict';

window.COB = window.COB || {};

window.COB.HostWebApp = function() {
    var hostWebUrl,
        appWebUrl,
        hostWebContext,
        destinationServerRelativeUrl,
        destinationFileName,
        
    // locate a file in the app web and retrieve the contents. If successful, provision to host web..
    readFromAppWebAndProvisionToHost = function (appPageUrl, hostWebServerRelativeUrl, hostWebFileName) {
        destinationServerRelativeUrl = hostWebServerRelativeUrl;
        destinationFileName = hostWebFileName;

        var req = $.ajax({
            url: appPageUrl,
            type: "GET",
            cache: false
        }).done(function (fileContents) {
            if (fileContents !== undefined && fileContents.length > 0) {
                uploadFileToHostWebViaCSOM(destinationServerRelativeUrl, destinationFileName, fileContents);
            }
            else {
                alert('Failed to read file from app web, so not uploading to host web..');
            }
        }).fail(function (jqXHR, textStatus) {
            alert("Request for page in app web failed: " + textStatus);
        });
    },

    // utility method for uploading files to host web..
    uploadFileToHostWebViaCSOM = function (serverRelativeUrl, filename, contents) {
        var createInfo = new SP.FileCreationInformation();
        createInfo.set_content(new SP.Base64EncodedByteArray());
        for (var i = 0; i < contents.length; i++) {

            createInfo.get_content().append(contents.charCodeAt(i));
        }
        createInfo.set_overwrite(true);
        createInfo.set_url(filename);
        var files = hostWebContext.get_web().getFolderByServerRelativeUrl(serverRelativeUrl).get_files();
        hostWebContext.load(files);
        files.add(createInfo);

        hostWebContext.executeQueryAsync(onProvisionFileSuccess, onProvisionFileFail);
    },
    onProvisionFileSuccess = function () {
        $('#message').append('<br /><div>File provisioned in host web successfully: ' + destinationServerRelativeUrl + '/' + destinationFileName + '</div>');
        setMaster('/' + destinationServerRelativeUrl + '/' + destinationFileName);
    },
    onProvisionFileFail = function (sender, args) {
        alert('Failed to provision file into host web. Error:' + sender.statusCode);
    },

    // set master page on host web..
    setMaster = function (masterUrl) {
        var hostWeb = hostWebContext.get_web();
        hostWeb.set_masterUrl(masterUrl);
        hostWeb.update();

        hostWebContext.load(hostWeb);
        hostWebContext.executeQueryAsync(onSetMasterSuccess, onSetMasterFail);
    },
    onSetMasterSuccess = function () {
        $('#message').append('<br /><div>Master page updated successfully..</div>');
    },
    onSetMasterFail = function (sender, args) {
        alert('Failed to update master page on host web. Error:' + args.get_message());
    },

    init = function () {
        var hostWebUrlFromQS = $.getUrlVar("SPHostUrl");
        hostWebUrl = (hostWebUrlFromQS !== undefined) ? decodeURIComponent(hostWebUrlFromQS) : undefined;

        var appWebUrlFromQS = $.getUrlVar("SPAppWebUrl");
        appWebUrl = (appWebUrlFromQS !== undefined) ? decodeURIComponent(appWebUrlFromQS) : undefined;
    }

    return {
        execute: function () {
            init();

            hostWebContext = new SP.ClientContext(window.COB.appHelper.getRelativeUrlFromAbsolute(hostWebUrl));
            readFromAppWebAndProvisionToHost(appWebUrl + '/MasterPages/MasterPageProvisionedByApp.txt', '_catalogs/masterpage', 'ProvisionedByApp.master');
        }
    }
}();

window.COB.AppHelper = {
    getRelativeUrlFromAbsolute: function (absoluteUrl) {
        absoluteUrl = absoluteUrl.replace('https://', '');
        var parts = absoluteUrl.split('/');
        var relativeUrl = '/';
        for (var i = 1; i < parts.length; i++) {
            relativeUrl += parts[i] + '/';
        }
        return relativeUrl;
    },
};

$(document).ready(function () {
    window.COB.HostWebApp.execute();
});


