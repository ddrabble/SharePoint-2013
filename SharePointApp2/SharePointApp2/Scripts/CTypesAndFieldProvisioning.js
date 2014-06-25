/// <reference path="jquery-1.7.1.js" />
/// <reference name="MicrosoftAjax.js" />
/// <reference path="~/_layouts/15/init.js" />
/// <reference path="~/_layouts/15/SP.Core.js" />
/// <reference path="~/_layouts/15/SP.Runtime.js" />
/// <reference path="~/_layouts/15/SP.UI.Dialog.js" />
/// <reference path="~/_layouts/15/SP.js" />

'use strict';

window.COB = window.COB || {};

window.COB.HostWebApp = function() {
    var hostWebUrl,
        appWebUrl,
        hostWebContext,
        hostWebContentTypes,
        createdField,
        createdFieldInternalName,
        createdContentTypeName,
        contentTypeName = 'COB Content Type',
        contentTypeDescription = 'Content Type provisioned from app',
        contentTypeGroupName = 'COB app content types',
        fieldName = 'COBProvisionedField',
        fieldDisplayName = 'Field provisioned by app',
        fieldGroupName = 'COB columns',
        
    createField = function (fieldType, fieldName, fieldDisplayName, fieldGroup, fieldHidden) {
        var fields = hostWebContext.get_web().get_fields();

        var fieldXml = "<Field Type='" + fieldType + "' DisplayName='" + fieldDisplayName + "' Name='" + fieldName + 
            "' Group='" + fieldGroup + "' Hidden='" + fieldHidden + "'></Field>";

        createdField = fields.addFieldAsXml(fieldXml, false, SP.AddFieldOptions.AddToNoContentType);

        hostWebContext.load(fields);
        hostWebContext.load(createdField);
        hostWebContext.executeQueryAsync(onProvisionFieldSuccess, onProvisionFieldFail);
    },
    onProvisionFieldSuccess = function () {
        $('#message').append('<br /><div>Field provisioned in host web successfully.</div>');
    },
    onProvisionFieldFail = function (sender, args) {
        alert('Failed to provision field into host web. Error:' + sender.statusCode);
    },

    createContentTypeInHost = function (ctypeName, ctypeDescription, ctypeGroup) {
        var hostWeb = hostWebContext.get_web();
        hostWebContentTypes = hostWeb.get_contentTypes();
        var cTypeInfo = new SP.ContentTypeCreationInformation();
        cTypeInfo.set_name(ctypeName);
        cTypeInfo.set_description(ctypeDescription);
        cTypeInfo.set_group(ctypeGroup);
        hostWebContentTypes.add(cTypeInfo);
        hostWebContext.load(hostWebContentTypes);
        hostWebContext.executeQueryAsync(onProvisionContentTypeSuccess, onProvisionContentTypeFail);
    },
    onProvisionContentTypeSuccess = function () {
        $('#message').append('<br /><div>Content type provisioned in host web successfully..</div>');

        // now the content type has been created, add the field..
        addFieldToContentTypeInHost(contentTypeName, fieldDisplayName);
    },
    onProvisionContentTypeFail = function (sender, args) {
        alert('Failed to provision content type into host web. Error:' + sender.statusCode);
    },

    addFieldToContentTypeInHost = function (ctypeName, fieldInternalName) {
        var hostWeb = hostWebContext.get_web();

        createdFieldInternalName = fieldInternalName;
        createdContentTypeName = ctypeName;

        // re-fetch created items..
        createdField = hostWeb.get_fields().getByInternalNameOrTitle(fieldInternalName);
        hostWebContext.load(createdField);
        
        hostWebContentTypes = hostWeb.get_contentTypes();
        hostWebContext.load(hostWebContentTypes);
       
        hostWebContext.executeQueryAsync(onItemsRefetchedSuccess, onItemsRefetchedFail);
        },
        onItemsRefetchedSuccess = function () {
            performAdd(createdContentTypeName, createdFieldInternalName);
        },
        onItemsRefetchedFail = function (sender, args) {
            alert('Failed to re-fetch field and content type. Error:' + sender.statusCode);
        },

        performAdd = function (ctypeName, fieldInternalName) {
            // iterate content types, find passed one, THEN add field..
            var cTypeFound = false;
            var createdContentType;

            var contentTypeEnumerator = hostWebContentTypes.getEnumerator();
            while (contentTypeEnumerator.moveNext()) {
                var contentType = contentTypeEnumerator.get_current();
                if (contentType.get_name() === ctypeName) {
                    cTypeFound = true;
                    createdContentType = contentType;
                    break;
                }
            }

            if (cTypeFound) {
                // - NOT the below line - SP.FieldCollection doesn't appear to have an add() method when fetched from content type..
                //contentType.get_fields.add(fieldInternalName)
                // - instead, this..
                var fieldRef = new SP.FieldLinkCreationInformation();
                fieldRef.set_field(createdField);
                
                createdContentType.get_fieldLinks().add(fieldRef);
                // specify push down..
                createdContentType.update(true);

                hostWebContext.load(createdContentType);
                hostWebContext.executeQueryAsync(onAddFieldToContentTypeSuccess, onAddFieldToContentTypeFail);
            }
            else {
                $('#message').append('<br /><div>Failed to add field to content type - check the content type exists!</div>');
            }
        },
        onAddFieldToContentTypeSuccess = function () {
            $('#message').append('<br /><div>Field added to content type in host web successfully..</div>');
        },
        onAddFieldToContentTypeFail = function (sender, args) {
            alert('Failed to add field to content type. Error:' + sender.statusCode);
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
            createField('Text', fieldName, fieldDisplayName, fieldGroupName, 'false');
            createContentTypeInHost(contentTypeName, contentTypeDescription, contentTypeGroupName);
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


