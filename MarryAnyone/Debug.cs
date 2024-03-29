﻿using System;
using TaleWorlds.Library;

namespace MarryAnyone
{
    internal static class Debug
    {
        public static void Print(string message)
        {
            MASettings settings = new();
            if (settings.Debug)
            {
                Color color = new(0.6f, 0.2f, 1f);
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }

        public static void Error(Exception exception)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Marry Anyone: {exception.Message}", Colors.Red));
        }
    }
}