﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaTokenizingTextBox.Controls;

/// <summary>
/// Class TokenizingWrapPanel
/// <para>Implements the <see cref="Avalonia.Controls.WrapPanel"/></para>
/// <para>Ported from https://github.com/iterate-ch/tokenizingtextbox </para>
/// </summary>
/// <seealso cref="Avalonia.Controls.WrapPanel"/>
public class TokenizingWrapPanel : WrapPanel
{
    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        RegisterStyledProperty<double>(nameof(HorizontalSpacing));

    public static readonly StyledProperty<Thickness> PaddingProperty =
        RegisterStyledProperty<Thickness>(nameof(Padding));

    public static readonly StyledProperty<StretchChild> StretchChildProperty =
        RegisterStyledProperty<StretchChild>(nameof(StretchChild));

    public static readonly StyledProperty<double> VerticalSpacingProperty =
        RegisterStyledProperty<double>(nameof(VerticalSpacing));

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public StretchChild StretchChild
    {
        get => GetValue(StretchChildProperty);
        set => SetValue(StretchChildProperty, value);
    }

    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }
    private struct UvMeasure
    {
        internal static readonly UvMeasure Zero = default;
        internal double U { get; set; }
        internal double V { get; set; }
        public UvMeasure(Orientation orientation, double width, double height) : this()
        {
            switch (orientation)
            {
                case Orientation.Horizontal:
                    U = width;
                    V = height;
                    break;
                case Orientation.Vertical:
                    U = height;
                    V = width;
                    break;
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UvMeasure parentMeasure = new(Orientation, finalSize.Width, finalSize.Height);
        UvMeasure spacingMeasure = new(Orientation, HorizontalSpacing, VerticalSpacing);
        UvMeasure paddingStart = new(Orientation, Padding.Left, Padding.Top);
        UvMeasure paddingEnd = new(Orientation, Padding.Right, Padding.Bottom);
        UvMeasure position = new(Orientation, Padding.Left, Padding.Top);

        double currentV = 0;
        void arrange(Control child, bool isLast = false)
        {
            if (child is Panel nestedPanel)
            {
                if (nestedPanel.Children.Count > 0)
                {
                    int nestedIndex = nestedPanel.Children.Count;
                    for (var i = 0; i < nestedIndex; i++)
                    {
                        arrange(nestedPanel.Children[i], isLast && (nestedIndex - i) == 1);
                    }
                }
                return;
            }

            UvMeasure desiredMeasure = new(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
            if (desiredMeasure.U == 0)
                return; // if an item is collapsed, avoid adding the spacing

            if ((desiredMeasure.U + position.U + paddingEnd.U) > parentMeasure.U)
            {
                //next row
                position.U = paddingStart.U;
                position.V += currentV + spacingMeasure.V;
                currentV = 0;
            }

            // Stretch the last item to fill the available space
            if (isLast && StretchChild == StretchChild.Last)
            {
                desiredMeasure.U = parentMeasure.U - position.U;
            }

            // place the item
            child.Arrange(Orientation == Orientation.Horizontal
                ? new Rect(position.U, position.V, desiredMeasure.U, desiredMeasure.V)
                : new Rect(position.V, position.U, desiredMeasure.V, desiredMeasure.U));

            // adjust the location for the next items
            position.U += desiredMeasure.U + spacingMeasure.U;
            currentV = Math.Max(desiredMeasure.V, currentV);
        }

        int lastIndex = Children.Count;
        for (var i = 0; i < lastIndex; i++)
        {
            arrange(Children[i], (lastIndex - i) == 1);
        }

        //return base.ArrangeOverride(finalSize);
        return finalSize;
    }

    protected override Size MeasureOverride(Size size)
    {
        double width = size.Width - Padding.Left - Padding.Right;
        double height = size.Height - Padding.Top - Padding.Bottom;

        Size availableSize = new(width, height);

        UvMeasure totalMeasure = UvMeasure.Zero;
        UvMeasure parentMeasure = new(Orientation, availableSize.Width, availableSize.Height);
        UvMeasure spacingMeasure = new(Orientation, HorizontalSpacing, VerticalSpacing);
        UvMeasure lineMeasure = UvMeasure.Zero;

        void measure(Avalonia.Controls.Controls elementCollection)
        {
            foreach (Control child in elementCollection)
            {
                if (child is Panel nestedPanel)
                {
                    measure(nestedPanel.Children);
                    continue;
                }

                child.Measure(availableSize);
                var currentMeasure = new UvMeasure(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                if (currentMeasure.U == 0)
                {
                    continue; // ignore collapsed items
                }

                // if this is the first item, do not add spacing. Spacing is added to the "left"
                double uChange = lineMeasure.U == 0
                    ? currentMeasure.U
                    : currentMeasure.U + spacingMeasure.U;
                if (parentMeasure.U >= uChange + lineMeasure.U)
                {
                    lineMeasure.U += uChange;
                    lineMeasure.V = Math.Max(lineMeasure.V, currentMeasure.V);
                }
                else
                {
                    // new line should be added
                    // to get the max U to provide it correctly to ui width ex: ---| or -----|
                    totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
                    totalMeasure.V += lineMeasure.V + spacingMeasure.V;

                    // if the next new row still can handle more controls
                    if (parentMeasure.U > currentMeasure.U)
                    {
                        // set lineMeasure initial values to the currentMeasure to be calculated later on the new loop
                        lineMeasure = currentMeasure;
                    }

                    // the control will take one row alone
                    else
                    {
                        // validate the new control measures
                        totalMeasure.U = Math.Max(currentMeasure.U, totalMeasure.U);
                        totalMeasure.V += currentMeasure.V;

                        // add new empty line
                        lineMeasure = UvMeasure.Zero;
                    }
                }
            }
        }
        measure(Children);

        // update value with the last line
        // if the the last loop is(parentMeasure.U > currentMeasure.U + lineMeasure.U) the total isn't calculated then calculate it
        // if the last loop is (parentMeasure.U > currentMeasure.U) the currentMeasure isn't added to the total so add it here
        // for the last condition it is zeros so adding it will make no difference
        // this way is faster than an if condition in every loop for checking the last item
        totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
        totalMeasure.V += lineMeasure.V;

        totalMeasure.U = Math.Ceiling(totalMeasure.U);

        return Orientation == Orientation.Horizontal ? new Size(totalMeasure.U, totalMeasure.V) : new Size(totalMeasure.V, totalMeasure.U);
    }

    private static StyledProperty<TValue> RegisterStyledProperty<TValue>(string name)
    {
#pragma warning disable AVP1001 // The same AvaloniaProperty should not be registered twice
        var prop = AvaloniaProperty.Register<TokenizingWrapPanel, TValue>(name);
#pragma warning restore AVP1001 // The same AvaloniaProperty should not be registered twice
        prop.Changed.Subscribe(eventArgs =>
        {
            if (eventArgs.Sender is WrapPanel wp)
            {
                wp.InvalidateMeasure();
                wp.InvalidateArrange();
            }
        });
        return prop;
    }
}