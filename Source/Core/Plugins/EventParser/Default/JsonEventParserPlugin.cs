﻿using System;
using System.Collections.Generic;
using CodeSmith.Core.Component;
using CodeSmith.Core.Extensions;
using Exceptionless.Core.Extensions;
using Exceptionless.Models;
using Newtonsoft.Json;

namespace Exceptionless.Core.Plugins.EventParser {
    [Priority(0)]
    public class JsonEventParserPlugin : IEventParserPlugin {
        public List<PersistentEvent> ParseEvents(string input) {
            var events = new List<PersistentEvent>();
            var serializerSettings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };

            switch (input.GetJsonType()) {
                case JsonType.Object: {
                    PersistentEvent ev;
                    if (input.TryFromJson(out ev, serializerSettings))
                        events.Add(ev);
                    break;
                }
                case JsonType.Array: {
                    PersistentEvent[] parsedEvents;
                    if (input.TryFromJson(out parsedEvents, serializerSettings))
                        events.AddRange(parsedEvents);
                    
                    break;
                }
            }

            return events.Count > 0 ? events : null;
        }
    }
}