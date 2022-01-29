using System.Runtime.Serialization;
using SadConsole;
using SadConsole.Effects;
using SadRogue.Primitives;

namespace LofiHollow {
    /// <summary>
    /// Switches between the glyph of a cell and a specified glyph for an amount of time, and then repeats.
    /// </summary>
    [DataContract]
    public class CustomBlink : CellEffectBase {
        private bool _isOn;
        private int _blinkCounter = 0;
        private System.TimeSpan _duration = System.TimeSpan.Zero;

         
        [DataMember]
        public System.TimeSpan BlinkSpeed { get; set; }
         
        [DataMember]
        public int GlyphIndex { get; set; }

        [DataMember]
        public Color BlinkColor { get; set; }
         
        [DataMember]
        public int BlinkCount { get; set; }
         
        [DataMember]
        public System.TimeSpan Duration { get; set; }
 
        public CustomBlink() {
            Duration = System.TimeSpan.MaxValue;
            BlinkCount = -1;
            BlinkSpeed = System.TimeSpan.FromSeconds(1);
            GlyphIndex = 0;
            BlinkColor = Color.White;
            _isOn = true;
        }

        public CustomBlink(int glyph, Color foreground) {
            Duration = System.TimeSpan.MaxValue;
            BlinkCount = -1;
            BlinkSpeed = System.TimeSpan.FromSeconds(1);
            GlyphIndex = glyph;
            BlinkColor = foreground;
        }

        /// <inheritdoc />
        public override bool ApplyToCell(ColoredGlyph cell, ColoredGlyphState originalState) {
            int oldGlyph = cell.Glyph;

            if (!_isOn) {
                cell.Glyph = GlyphIndex;
                cell.Foreground = BlinkColor;
            } else {
                cell.Glyph = originalState.Glyph;
                cell.Foreground = originalState.Foreground;
            }
            return cell.Glyph != oldGlyph;
        }

        /// <inheritdoc />
        public override void Update(System.TimeSpan delta) {
            base.Update(delta);

            if (_delayFinished && !IsFinished) {
                if (Duration != System.TimeSpan.MaxValue) {
                    _duration += delta;
                    if (_duration >= Duration) {
                        IsFinished = true;
                        return;
                    }
                }

                if (_timeElapsed >= BlinkSpeed) {
                    _isOn = !_isOn;
                    _timeElapsed = System.TimeSpan.Zero;

                    if (BlinkCount != -1) {
                        _blinkCounter += 1;

                        if (BlinkCount != -1 && _blinkCounter > (BlinkCount * 2))
                            IsFinished = true;
                    }
                }

                if (_timeElapsed >= BlinkSpeed) {
                    _isOn = !_isOn;
                    _timeElapsed = System.TimeSpan.Zero;
                }
            }
        } 
        public override void Restart() {
            _isOn = true;
            _blinkCounter = 0;
            _duration = System.TimeSpan.Zero;

            base.Restart();
        }
         
        public override ICellEffect Clone() => new CustomBlink() {
            _isOn = _isOn,
            BlinkSpeed = BlinkSpeed,
            GlyphIndex = GlyphIndex,

            IsFinished = IsFinished,
            StartDelay = StartDelay,
            CloneOnAdd = CloneOnAdd,
            RemoveOnFinished = RemoveOnFinished,
            RestoreCellOnRemoved = RestoreCellOnRemoved,
            _timeElapsed = _timeElapsed,
        };

        //public override bool Equals(ICellEffect effect)
        //{
        //    if (effect is BlinkGlyph)
        //    {
        //        if (base.Equals(effect))
        //        {
        //            var effect2 = (BlinkGlyph)effect;

        //            return GlyphIndex == effect2.GlyphIndex &&
        //                   BlinkSpeed == effect2.BlinkSpeed &&
        //                   RemoveOnFinished == effect2.RemoveOnFinished &&
        //                   StartDelay == effect2.StartDelay;
        //        }
        //    }

        //    return false;
        //}

        /// <inheritdoc />
        public override string ToString() =>
            string.Format("BLINKCHAR-{0}-{1}-{2}-{3}", GlyphIndex, BlinkSpeed, StartDelay, RemoveOnFinished);
    }
}