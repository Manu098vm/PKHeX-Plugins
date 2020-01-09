﻿using System;
using System.Windows.Forms;
using PKHeX.Core;

namespace AutoModPlugins
{
    public class GPSSPlugin : AutoModPlugin
    {
        public override string Name => "GPSS Tools";
        public override int Priority => 1;

        protected override void AddPluginControl(ToolStripDropDownItem modmenu)
        {
            var ctrl = new ToolStripMenuItem(Name) { Image = Properties.Resources.legalizeboxes };

            var c1 = new ToolStripMenuItem("Upload to GPSS") { Image = Properties.Resources.legalizeboxes };
            c1.Click += GPSSUpload;
            var c2 = new ToolStripMenuItem("Import from GPSS URL") { Image = Properties.Resources.legalizeboxes };
            c2.Click += GPSSDownload;

            ctrl.DropDownItems.Add(c1);
            ctrl.DropDownItems.Add(c2);
            modmenu.DropDownItems.Add(ctrl);
        }

        private void GPSSUpload(object sender, EventArgs e)
        {
            var pk = PKMEditor.PreparePKM();
            byte[] rawdata = pk.Data;
            var postval = PKHeX.Core.AutoMod.NetUtil.GPSSPost(rawdata);
            Clipboard.SetText(postval);
            WinFormsUtil.Alert(postval);
        }

        private void GPSSDownload(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                var txt = Clipboard.GetText();
                if (!txt.Contains("/gpss/")) 
                {
                    WinFormsUtil.Error("Invalid URL or incorrect data in the clipboard");
                    return;
                }
                else
                {
                    if (long.TryParse(txt.Split('/')[txt.Split('/').Length - 1], out long code))
                    {
                        var pkbytes = PKHeX.Core.AutoMod.NetUtil.GPSSDownload(code);
                        var pkm = PKMConverter.GetPKMfromBytes(pkbytes);
                        if (LoadPKM(pkm))
                            WinFormsUtil.Alert("GPSS Pokemon loaded to PKM Editor");
                        else
                            WinFormsUtil.Error("Error parsing PKM bytes. Make sure the pokemon is valid and can exist in this generation.");
                    }
                    else
                    {
                        WinFormsUtil.Error("Invalid URL (wrong code)");
                        return;
                    }
                }
            }
        }

        private bool LoadPKM(PKM pk)
        {
            pk = PKMConverter.ConvertToType(pk, SaveFileEditor.SAV.PKMType, out string c);
            if (pk == null)
                return false;
            PKMEditor.PopulateFields(pk);
            return true;
        }

    }
}
