<%@ Control Language="C#" CodeFile="DisplaySign.ascx.cs" CodeBehind="DisplaySign.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.DisplayBoard" %>

<script type="text/javascript">
var lastID = "";

function loadData(data)
{
    var obj;

    if (typeof(data["URL"]) != "undefined" && data["URL"] != '')
    {
        $('#mainHolder').rsfSlideshow('removeSlides');
        $('#mainHolder').rsfSlideshow('addSlides', Array({ url: data["URL"] }));
        $('#mainHolder').rsfSlideshow('goToSlide', 0);
    }
    else
    {
        $('#mainHolder').rsfSlideshow('removeSlides');
        $('#mainHolder').rsfSlideshow('addSlides', Array({ url: 'UserControls/Custom/HDC/Misc/Images/DisplaySignBlank.png' }));
        $('#mainHolder').rsfSlideshow('goToSlide', 0);
    }

    if (typeof(data["ID"]) == "undefined")
        lastID = "";
    else {
        lastID = data["ID"];

        if (lastID != "" && typeof(window.Kiosk) != "undefined")
            window.Kiosk.UpdateSystemActivity();
    }
}

function loadNextPage()
{
    if (typeof(lastID) != "string")
        lastID = "";

    $.get("<%= Request.RawUrl %>", { format: "xml", lastID: lastID }, parseHtml, "html");
}

function parseHtml(xml)
{
    var data = new Object;

    data["ID"] = $.trim($(xml).find("ID").text());
    data["URL"] = $.trim($(xml).find("URL").text());

    loadData(data);
}

$(document).ready(function() {
    $('#mainHolder').rsfSlideshow({ autostart: false, loop: false, transition: <%= TransitionTimeSetting %> });

    loadNextPage();
    setInterval("loadNextPage();", <%= SlideTimeSetting * 1000 %>);
});
</script>

<div id="mainHolder" class="rs-slideshow">
    <div class="slide-container">
    </div>

</div>
