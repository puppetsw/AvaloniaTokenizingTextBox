using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System;

namespace AvaloniaTokenizingTextBox.Controls
{
    public partial class TokenizingWrapPanel : WrapPanel
    {
        public static readonly StyledProperty<double> HorizontalSpacingProperty =
            AvaloniaProperty.Register<TokenizingWrapPanel, double>(nameof(HorizontalSpacing));

        public static readonly StyledProperty<double> VerticalSpacingProperty =
            AvaloniaProperty.Register<TokenizingWrapPanel, double>(nameof(VerticalSpacing));

        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<TokenizingWrapPanel, Thickness>(nameof(Padding));

        public static readonly StyledProperty<StretchChild> StretchChildProperty =
            AvaloniaProperty.Register<TokenizingWrapPanel, StretchChild>(nameof(StretchChild));

        public double HorizontalSpacing
        {
            get { return GetValue(HorizontalSpacingProperty); }
            set { SetValue(HorizontalSpacingProperty, value); }
        }
        public double VerticalSpacing
        {
            get { return GetValue(VerticalSpacingProperty); }
            set { SetValue(VerticalSpacingProperty, value); }
        }

        public Thickness Padding
        {
            get { return GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }
        public StretchChild StretchChild
        {
            get { return GetValue(StretchChildProperty); }
            set { SetValue(StretchChildProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count > 0)
            {
                UvMeasure parentMeasure = new UvMeasure(Orientation, finalSize.Width, finalSize.Height);
                var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
                var paddingStart = new UvMeasure(Orientation, Padding.Left, Padding.Top);
                var paddingEnd = new UvMeasure(Orientation, Padding.Right, Padding.Bottom);
                var position = new UvMeasure(Orientation, Padding.Left, Padding.Top);


                double currentV = 0;
                void arrange(IControl child, bool isLast = false)
                {
                    if (child is Panel nestedPanel)
                    {
                        if (nestedPanel.Children.Count > 0)
                        {
                            var nestedIndex = nestedPanel.Children.Count;
                            for (var i = 0; i < nestedIndex; i++)
                            {
                                arrange(nestedPanel.Children[i], isLast && (nestedIndex - i) == 1);
                            }
                        }
                        return;
                    }

                    var desiredMeasure = new UvMeasure(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
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
                    if (Orientation == Orientation.Horizontal)
                    {
                        child.Arrange(new Rect(position.U, position.V, desiredMeasure.U, desiredMeasure.V));
                    }
                    else
                    {
                        child.Arrange(new Rect(position.V, position.U, desiredMeasure.V, desiredMeasure.U));
                    }

                    // adjust the location for the next items
                    position.U += desiredMeasure.U + spacingMeasure.U;
                    currentV = Math.Max(desiredMeasure.V, currentV);
                }

                var lastIndex = Children.Count;
                for (var i = 0; i < lastIndex; i++)
                {
                    arrange(Children[i], (lastIndex - i) == 1);
                }
            }

            //return base.ArrangeOverride(finalSize);

            return finalSize;
        }

        protected override Size MeasureOverride(Size size)
        {
            double width = size.Width - Padding.Left - Padding.Right;
            double height = size.Height - Padding.Top - Padding.Bottom;

            Size availableSize = new Size(width, height);

            var totalMeasure = UvMeasure.Zero;
            var parentMeasure = new UvMeasure(Orientation, availableSize.Width, availableSize.Height);
            var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
            var lineMeasure = UvMeasure.Zero;

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

        private struct UvMeasure
        {
            internal static readonly UvMeasure Zero = default;

            public UvMeasure(Orientation orientation, double width, double height) : this()
            {
                if (orientation == Orientation.Horizontal)
                {
                    U = width;
                    V = height;
                }

                if (orientation == Orientation.Vertical)
                {
                    U = height;
                    V = width;
                }
            }

            internal double U { get; set; }
            internal double V { get; set; }
        }
    }
}