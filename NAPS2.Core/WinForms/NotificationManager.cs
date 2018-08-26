﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Config;
using NAPS2.Operation;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class NotificationManager : ISaveNotify
    {
        private const int PADDING_X = 25, PADDING_Y = 25;
        private const int SPACING_Y = 20;

        private readonly AppConfigManager appConfigManager;

        private readonly List<NotifyWidget> slots = new List<NotifyWidget>();
        private FormBase parentForm;

        public NotificationManager(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
        }

        public FormBase ParentForm
        {
            get => parentForm;
            set
            {
                parentForm = value;
                parentForm.Resize += parentForm_Resize;
            }
        }

        public void PdfSaved(string path)
        {
            Show(new PdfSavedNotifyWidget(path));
        }

        public void ImagesSaved(int imageCount, string path)
        {
            if (imageCount == 1)
            {
                Show(new OneImageSavedNotifyWidget(path));
            }
            else if (imageCount > 1)
            {
                Show(new ImagesSavedNotifyWidget(imageCount, path));
            }
        }

        public void DonatePrompt()
        {
            Show(new DonatePromptNotifyWidget());
        }

        public void OperationProgress(IOperationProgress opModalProgress, IOperation op)
        {
            //Show(new OperationProgressNotifyWidget(opManager, op));
        }

        private void Show(NotifyWidget n)
        {
            if (appConfigManager.Config.DisableSaveNotifications)
            {
                return;
            }

            int slot = FillNextSlot(n);
            n.Location = GetPosition(n, slot);
            n.BringToFront();
            n.HideNotify += (sender, args) => ClearSlot(n);
            n.ShowNotify();
        }

        private void parentForm_Resize(object sender, EventArgs e)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].Location = GetPosition(slots[i], i);
                }
            }
        }

        private void ClearSlot(NotifyWidget n)
        {
            var index = slots.IndexOf(n);
            if (index != -1)
            {
                parentForm.Controls.Remove(n);
                slots[index] = null;
            }
        }

        private int FillNextSlot(NotifyWidget n)
        {
            var index = slots.IndexOf(null);
            if (index == -1)
            {
                index = slots.Count;
                slots.Add(n);
            }
            else
            {
                slots[index] = n;
            }
            parentForm.Controls.Add(n);
            return index;
        }

        private Point GetPosition(NotifyWidget n, int slot)
        {
            return new Point(parentForm.ClientSize.Width - n.Width - PADDING_X,
                parentForm.ClientSize.Height - n.Height - PADDING_Y - (n.Height + SPACING_Y) * slot);
        }
    }
}
