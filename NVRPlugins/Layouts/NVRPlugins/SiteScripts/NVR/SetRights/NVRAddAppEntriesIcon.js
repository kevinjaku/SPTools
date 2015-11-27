﻿(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};


    function getUrlParts(url) {
        var a = document.createElement('a');
        a.href = url;

        return {
            href: a.href,
            host: a.host,
            hostname: a.hostname,
            port: a.port,
            pathname: a.pathname,
            protocol: a.protocol,
            hash: a.hash,
            search: a.search
        };
    }

    var tmpsrc = get_url_param("Source");
    var relSource = getUrlParts(decodeURIComponent(tmpsrc)).pathname;
    console.log("relSource", relSource);

    window["NVRAddAppEntries"]["SetRights"] =
    {
        "Name": "SetRights",
        "IconUrl": "/_layouts/15/images/LTOBJECT.PNG?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true&NVR_SiteConfigActiveFor=" + encodeURIComponent(ContextGuids.userlogin) 
            + "&NVR_SiteConfigApp=SetRights--I%0AItemUpdated%0AItemAdded&Title=SetRights%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=Receivers"
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent('{"ItemUpdated": {"SetRights--I": "SetRights"},"ItemAdded": {"SetRights--I": "SetRights"} }')
            + "&ContentTypeId=0x010000AF00A19FBA41D68E7FDB37BCB1847B",
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=/NVR/SetRights/Info.html",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();