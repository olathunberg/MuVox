﻿//
// Based on FastAttckCompressor1177, SkypeVoiceChanger (http://skypefx.codeplex.com/)
//
// --------------------------------------------------------------------------------------------------------
//
// Copyright 2006, Thomas Scott Stillwell
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
// 
// Redistributions of source code must retain the above copyright notice, this list of conditions
// and the following disclaimer.
// 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions
// and the following disclaimer in the documentation and/or other materials provided with the distribution.
// 
// The name of Thomas Scott Stillwell may not be used to endorse or
// promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS
// BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// adapted to .NET by Mark Heath

using NAudio.Wave;
using System;

namespace TTech.MuVox.Features.Processor.SampleProviders
{
    public class FastAttackCompressor1175 : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;

        private readonly float autogain = 0;
        private readonly float rmscoef = 0;
        private readonly float softknee = 0;

        private float allin;
        private float atcoef;
        private float attime;
        private float averatio;
        private float cratio;
        private float cthresh;
        private float cthreshv;
        private float db2log;
        private float log2db;
        private float makeup;
        private float makeupv;
        private float maxover;
        private float mix;
        private float overdb;
        private float ratatcoef;
        private float ratio;
        private float ratrelcoef;
        private float relcoef;
        private float reltime;
        private float runave;
        private float rundb;
        private float runmax;
        private float runratio;

        public FastAttackCompressor1175(ISampleProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
            SampleRate = 44100;

            Threshold = 0; // min -60, max 0
            Ratio = 1; // min 0, max 3
            Gain = 0; // min -20, max 20
            Attack = 20; // min 20, max 2000
            Release = 250; // min 20, max 1000
            Mix = 100; // min 0, max 100

            Init();
        }

        public int Attack { get; set; }

        public float Gain { get; set; }

        public float Mix { get; set; }

        public int Ratio { get; set; }

        public int Release { get; set; }

        public float SampleRate { get; set; }

        public float Threshold { get; set; }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public void Init()
        {
            log2db = 8.6858896380650365530225783783321f; // 20 / ln(10)
            db2log = 0.11512925464970228420089957273422f; // ln(10) / 20
            attime = 0.010f;
            reltime = 0.100f;
            ratio = 0;
            cratio = 0;
            rundb = 0;
            overdb = 0;
            ratatcoef = (float)Math.Exp(-1 / (0.00001f * SampleRate));
            ratrelcoef = (float)Math.Exp(-1 / (0.5f * SampleRate));
            atcoef = (float)Math.Exp(-1 / (attime * SampleRate));
            relcoef = (float)Math.Exp(-1 / (reltime * SampleRate));
            mix = 1;

            switch (Ratio)
            {
                case 0: ratio = 4; break;
                case 1: ratio = 8; break;
                case 2: ratio = 12; break;
                case 3: ratio = 20; break;
                case 4:
                    allin = 1;
                    cratio = 20;
                    ratio = 20;
                    break;
                default:
                    allin = 0;
                    cratio = ratio;
                    break;
            }

            cthresh = (Math.Abs(softknee) > double.Epsilon) ? (Threshold - 3) : Threshold;
            cthreshv = (float)Math.Exp(cthresh * db2log);
            makeup = Gain;
            makeupv = (float)Math.Exp((makeup + autogain) * db2log);
            attime = Attack / 1000000f;
            reltime = Release / 1000f;
            atcoef = (float)Math.Exp(-1 / (attime * SampleRate));
            relcoef = (float)Math.Exp(-1 / (reltime * SampleRate));
            mix = Mix / 100;
        }
      
        public int Read(float[] buffer, int offset, int count)
        {
            int read = sourceProvider.Read(buffer, offset, count);

            Process(buffer, offset, read);

            return read;
        }

        private void OnSample(ref float left, ref float right)
        {
            float ospl0 = left;
            float ospl1 = right;
            float aspl0 = Math.Abs(left);
            float aspl1 = Math.Abs(right);
            float maxspl = Math.Max(aspl0, aspl1);
            maxspl *= maxspl;
            runave = maxspl + (rmscoef * (runave - maxspl));
            var det = (float)Math.Sqrt(Math.Max(0f, runave));

            overdb = 2.08136898f * (float)Math.Log(det / cthreshv) * log2db;
            overdb = Math.Max(0, overdb);

            if (overdb - rundb > 5) averatio = 4;

            if (overdb > rundb)
            {
                rundb = overdb + atcoef * (rundb - overdb);
                runratio = averatio + ratatcoef * (runratio - averatio);
            }
            else
            {
                rundb = overdb + relcoef * (rundb - overdb);
                runratio = averatio + ratrelcoef * (runratio - averatio);
            }
            overdb = rundb;
            averatio = runratio;

            if (Math.Abs(allin) > double.Epsilon)
                cratio = 12 + averatio;
            else
                cratio = ratio;

            var gr = -overdb * (cratio - 1) / cratio;
            var grv = (float)Math.Exp(gr * db2log);

            runmax = maxover + relcoef * (runmax - maxover);  // highest peak for setting att/rel decays in reltime
            maxover = runmax;

            left *= grv * makeupv * mix;
            right *= grv * makeupv * mix;

            left += ospl0 * (1 - mix);
            right += ospl1 * (1 - mix);
        }
        
        private void Process(float[] buffer, int offset, int count)
        {
            int samples = count;

            for (int sample = 0; sample < samples; sample++)
            {
                float sampleLeft = buffer[offset];
                float sampleRight = sampleLeft;
                if (WaveFormat.Channels == 2)
                {
                    sampleRight = buffer[offset + 1];
                    sample++;
                }

                OnSample(ref sampleLeft, ref sampleRight);

                // put them back
                buffer[offset++] = sampleLeft;
                if (WaveFormat.Channels == 2)
                {
                    buffer[offset++] = sampleRight;
                }
            }
        }
    }
}
