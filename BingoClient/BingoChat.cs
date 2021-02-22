using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.BingoClient {
    public class BingoChat {
        private const float ChatTime = 4f;
        private List<string> ChatMessages = new List<string>();
        private List<string> ChatHistory = new List<string>();
        private List<float> ChatTimers = new List<float>();

        private ButtonBinding OpenChat;

        public bool ChatOpen;
        private string Buffer = "";
        private bool Underscore;
        private float UnderscoreCounter;
        private Action<string> Submit;

        public BingoChat(ButtonBinding openChat, Action<string> submit) {
            this.OpenChat = openChat;
            this.Submit = submit;
            TextInput.OnInput += OnInput;
        }

        public void Update() {
            for (int i = 0; i < this.ChatTimers.Count; i++) {
                this.ChatTimers[i] += Engine.RawDeltaTime;
                if (this.ChatTimers[i] > ChatTime) {
                    this.ChatTimers.RemoveAt(i);
                    this.ChatMessages.RemoveAt(i);
                    i--;
                }
            }

            if (this.ChatOpen) {
                if (Input.ESC.Pressed) {
                    this.ChatOpen = false;
                }
            } else {
                if (this.OpenChat.Pressed && 
                    !Engine.Commands.Open && 
                    !(Engine.Scene is Overworld overworld &&
                      (overworld.Current is OuiModOptionString || overworld.Current is OuiFileNaming))) {
                    this.ChatOpen = true;
                }
            }
            
            this.UnderscoreCounter += Engine.RawDeltaTime;
            while (this.UnderscoreCounter >= 0.5f) {
              this.UnderscoreCounter -= 0.5f;
              this.Underscore = !this.Underscore;
            }
        }

        private void OnInput(char ch) {
            this.Underscore = false;
            this.UnderscoreCounter = 0f;
            if (ch == '\r' || ch == '\n') {
                var buf = this.Buffer;
                this.Buffer = "";
                this.Submit(buf);
            } else if (ch == '\b') {
                if (this.Buffer.Length > 0) {
                    this.Buffer = this.Buffer.Substring(0, this.Buffer.Length - 1);
                }
            } else if (!char.IsControl(ch)) {
                this.Buffer += ch;
            }
        }

        public void Render() {
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                Engine.ScreenMatrix);

            var currentBase = 1080f - 50f;
            var scale = 0.7f;
            var chatTexts = this.ChatOpen ? this.ChatHistory : this.ChatMessages;
            var chatTimers = this.ChatOpen ? null : this.ChatTimers;
            for (int i = chatTexts.Count - 1; i >= 0 && currentBase > 0; i--) {
                var timer = chatTimers?[i] ?? ChatTime / 2f;
                var text = chatTexts[i];
                var alpha = timer < 0.25f ? timer * 4f : timer > (ChatTime - 0.5f) ? (ChatTime - timer) * 2 : 1f;
                var rise = timer < 0.25f ? timer * 4f : 1f;

                var textSize = ActiveFont.Measure(text) * scale;
                ActiveFont.DrawOutline(text, new Vector2(1920f - 20, currentBase), new Vector2(1f, rise), Vector2.One * scale, Color.White * alpha, 2f, Color.Black * alpha);

                currentBase -= textSize.Y * 1.1f * rise;
            }

            var uch = this.Underscore ? "_" : "";
            var prompt = $"> {this.Buffer}{uch}";
            ActiveFont.DrawOutline(prompt, new Vector2(0, 1080f - 10), new Vector2(0f, 1f), Vector2.One * scale, Color.White, 2f, Color.Black);
            Draw.SpriteBatch.End();
        }
        
        public void Chat(string text) {
            this.ChatHistory.Add(text);
            this.ChatMessages.Add(text);
            this.ChatTimers.Add(0f);
        }

        public void SetHistory(List<string> history) {
            this.ChatHistory.Clear();
            this.ChatHistory.AddRange(history);
        }
    }
}
