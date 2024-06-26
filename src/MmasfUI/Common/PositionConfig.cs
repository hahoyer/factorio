﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;
using JetBrains.Annotations;

namespace MmasfUI.Common;

public sealed class PositionConfig : IDisposable
{
    readonly Func<string> GetFileNameFunction;
    Window TargetValue;
    bool LoadPositionCalled;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="getFileName">
    ///     function to obtain filename of configuration file.
    ///     <para>It will be called each time the name is required. </para>
    ///     <para>Default: Target.Name</para>
    /// </param>
    public PositionConfig
        (Func<string> getFileName = null)
        => GetFileNameFunction = getFileName ?? (() => TargetValue?.Name.ToValidFileName());

    void IDisposable.Dispose() => Disconnect();

    /// <summary>
    ///     Form that will be controlled by this instance
    /// </summary>
    public Window Target
    {
        [UsedImplicitly]
        get => TargetValue;
        set
        {
                Disconnect();
                TargetValue = value;
                Connect();
            }
    }

    /// <summary>
    ///     Name that will be used as filename
    /// </summary>
    public string FileName => GetFileNameFunction();

    Rect? Position
    {
        get => Convert(0, null, s => s.FromJson<Rect?>());
        set => Save(value, WindowState);
    }

    string[] ParameterStrings => TargetValue == null? null : FileHandle.String?.Split('\n');

    SmbFile FileHandle => FileName.ToSmbFile();

    WindowState WindowState
    {
        get => Convert(1, WindowState.Normal, s => s.Parse<WindowState>());
        set => Save(Position, value);
    }

    void Disconnect()
    {
            if(TargetValue == null)
                return;

            //TargetValue.SuspendLayout();
            LoadPositionCalled = false;
            TargetValue.Loaded -= OnLoad;
            TargetValue.LocationChanged -= OnLocationChanged;
            TargetValue.SizeChanged -= OnLocationChanged;
            //TargetValue.ResumeLayout();
            TargetValue = null;
        }

    void Connect()
    {
            if(TargetValue == null)
                return;
            //TargetValue.SuspendLayout();
            LoadPositionCalled = false;
            TargetValue.Loaded += OnLoad;
            TargetValue.LocationChanged += OnLocationChanged;
            TargetValue.SizeChanged += OnLocationChanged;
            //TargetValue.ResumeLayout();
        }

    void OnLocationChanged(object target, EventArgs e)
    {
            if(target != TargetValue)
                return;

            SavePosition();
        }

    void OnLoad(object target, EventArgs e)
    {
            if(target != TargetValue)
                return;

            LoadPosition();
        }

    void Save(Rect? position, WindowState state)
    {
            var fileHandle = FileHandle;
            (fileHandle != null).Assert();
            fileHandle.String = "{0}\n{1}"
                .ReplaceArgs
                (
                    position == null? "" : position.Value.ToJSon(),
                    state
                );
        }

    T Convert<T>
        (int position, T defaultValue, Func<string, T> converter)
        => ParameterStrings == null || ParameterStrings.Length <= position
            ? defaultValue
            : converter(ParameterStrings[position]);

    void LoadPosition()
    {
            var fileHandle = FileHandle;
            (fileHandle != null).Assert();
            if(fileHandle.String != null)
            {
                var position = Position;
                (position != null).Assert();
                //TargetValue.SuspendLayout();
                TargetValue.WindowStartupLocation = WindowStartupLocation.Manual;
                var rect = EnsureVisible(position.Value);
                TargetValue.Left = rect.Left;
                TargetValue.Top = rect.Top;
                TargetValue.Width = rect.Width;
                TargetValue.Height = rect.Height;
                TargetValue.WindowState = WindowState;
                //TargetValue.ResumeLayout(true);
            }

            LoadPositionCalled = true;
        }

    void SavePosition()
    {
            if(!LoadPositionCalled)
                return;

            if(TargetValue.WindowState == WindowState.Normal)
                Position = TargetValue.RestoreBounds;

            WindowState = TargetValue.WindowState;
        }

    static Rectangle EnsureVisible(Rect valueRect)
    {
            var value = valueRect.ToRectangle();
            var allScreens = Screen.AllScreens;
            if(allScreens.Any(s => s.Bounds.IntersectsWith(value)))
                return value;

            var closestScreen = Screen.FromRectangle(value);
            var result = value;

            var leftDistance = value.Left - closestScreen.Bounds.Right;
            var rightDistance = value.Right - closestScreen.Bounds.Left;

            if(leftDistance > 0 && rightDistance > 0)
                result.X += leftDistance < rightDistance? -(leftDistance + 10) : rightDistance + 10;

            var topDistance = value.Top - closestScreen.Bounds.Bottom;
            var bottomDistance = value.Bottom - closestScreen.Bounds.Top;

            if(topDistance > 0 && bottomDistance > 0)
                result.Y += topDistance < bottomDistance? -(topDistance + 10) : bottomDistance + 10;

            closestScreen.Bounds.IntersectsWith(result).Assert();
            return result;
        }
}