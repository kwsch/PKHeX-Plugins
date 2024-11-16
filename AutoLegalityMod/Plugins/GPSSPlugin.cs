using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using AutoModPlugins.Properties;
using PKHeX.Core;
using System.Net.Http;

namespace AutoModPlugins;

public class GPSSPlugin : AutoModPlugin
{
    public override string Name => "GPSS Tools";
    public override int Priority => 2;
    public static string Url => _settings.GPSSBaseURL;

    protected override void AddPluginControl(ToolStripDropDownItem modmenu)
    {
        var ctrl = new ToolStripMenuItem(Name)
        {
            Name = "Menu_GPSSPlugin",
            Image = Resources.flagbrew,
        };
        var c1 = new ToolStripMenuItem("Upload to GPSS") { Image = Resources.uploadgpss };
        var c2 = new ToolStripMenuItem("Import from GPSS URL")
        {
            Image = Resources.mgdbdownload,
        };
        c1.Click += GPSSUpload;
        c1.Name = "Menu_UploadtoGPSS";
        c2.Click += GPSSDownload;
        c2.Name = "Menu_ImportfromGPSSURL";

        ctrl.DropDownItems.Add(c1);
        ctrl.DropDownItems.Add(c2);
        modmenu.DropDownItems.Add(ctrl);
    }

    private async void GPSSUpload(object? sender, EventArgs e)
    {
        try
        {
            var pk = PKMEditor.PreparePKM();
            try
            {
                var response = await PKHeX.Core.Enhancements.NetUtil.GPSSPost(pk.DecryptedPartyData, pk.Format, Url);
                var content = await response.Content.ReadAsStringAsync();
                var decoded = JsonSerializer.Deserialize<JsonNode>(content);
                if (decoded == null)
                    return;

                string msg = GetResult(decoded, response, out bool copyToClipboard);
                if (copyToClipboard)
                    Clipboard.SetText($"https://{Url}/gpss/{decoded["code"]}");
                WinFormsUtil.Alert(msg);
            }
            catch (Exception ex)
            {
                WinFormsUtil.Alert($"Something went wrong uploading to GPSS.\nError details: {ex.Message}");
            }
        }
        catch
        {
            // Ignore.
        }
    }

    private static string GetResult(JsonNode decoded, HttpResponseMessage response, out bool copyToClipboard)
    {
        var error = decoded["error"] is { } x ? x.ToString() : null;
        copyToClipboard = false;

        // TODO set proper status codes on FlagBrew side - Allen;
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                return "Uploading to GPSS is currently disabled, please try again later, or check the FlagBrew discord for more information.";
            return $"Uploading to GPSS returned an unexpected status code {response.StatusCode}\nError details (if any returned from server): {error}";
        }

        switch (error)
        {
            case "" or null or "no errors":
                return $"Pokemon added to the GPSS database. Here is your URL (has been copied to the clipboard):\n https://{Url}/gpss/{decoded["code"]}";

            case "your pokemon is being held for manual review":
                copyToClipboard = true;
                return $"Your Pokémon was uploaded to GPSS, however it is being held for manual review. Once approved it will be available at https://{Url}/gpss/{decoded["code"]} (copied to clipboard)";

            case "Your Pokemon is already uploaded":
                copyToClipboard = true;
                return $"Your Pokémon was already uploaded to GPSS, and is available at https://{Url}/gpss/{decoded["code"]} (copied to clipboard)";

            default:
                return $"Could not upload your Pokemon to GPSS, please try again later or ask Allen if something seems wrong.\n Error details: {decoded["code"]}";
        }
    }

    private void GPSSDownload(object? sender, EventArgs e)
    {
        if (Clipboard.ContainsText())
        {
            var txt = Clipboard.GetText();
            if (!txt.Contains("/gpss/"))
            {
                WinFormsUtil.Error("Invalid URL or incorrect data in the clipboard");
                return;
            }

            if (!long.TryParse(txt.Split('/')[^1], out long code))
            {
                WinFormsUtil.Error("Invalid URL (wrong code)");
                return;
            }

            var pkbytes = PKHeX.Core.Enhancements.NetUtil.GPSSDownload(code, Url);
            if (pkbytes == null)
            {
                WinFormsUtil.Error("GPSS Download failed");
                return;
            }
            var pkm = EntityFormat.GetFromBytes(pkbytes, EntityContext.None);
            if (pkm == null || !LoadPKM(pkm))
            {
                WinFormsUtil.Error("Error parsing PKM bytes. Make sure the Pokémon is valid and can exist in this generation.");
                return;
            }
            WinFormsUtil.Alert("GPSS Pokemon loaded to PKM Editor");
        }
    }

    private bool LoadPKM(PKM pk)
    {
        var result = EntityConverter.ConvertToType(pk, SaveFileEditor.SAV.PKMType, out _);
        if (result == null)
        {
            return false;
        }

        PKMEditor.PopulateFields(result);
        return true;
    }
}