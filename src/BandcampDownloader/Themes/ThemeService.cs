﻿using System;
using System.Windows;
using BandcampDownloader.Core;

namespace BandcampDownloader.Themes;

internal interface IThemeService
{
    /// <summary>
    /// Applies the specified <see cref="Skin"/> to the application resources.
    /// </summary>
    void ApplySkin(Skin skin);
}

internal sealed class ThemeService : IThemeService
{
    public void ApplySkin(Skin skin)
    {
        Application.Current.Resources.MergedDictionaries.Clear();

        switch (skin)
        {
            case Skin.Dark:
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes\\DarkTheme.xaml", UriKind.Relative) });
                break;
            case Skin.Light:
                // Do nothing
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
