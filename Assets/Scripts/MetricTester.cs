using OpenTelemetry.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTelemetry.Unity.Examples 
{ 

    public class MetricTester : MonoBehaviour
    {
        [SerializeField]
        private SimpleTelemetryConfig _config;

        TracerProvider _tracerProvider;

        JsonExporterOptions _jsonOptions;

        // Start is called before the first frame update
        void Awake()
        {
            Init();
        }

        public void Init()
        {

            var jsonOptions = new JsonExporterOptions()
            {
                WriteToApi = false,
                WriteToFile = true,
            };
            if(_config)
            {
                jsonOptions.WriteToApi = _config.WriteToApi;
                jsonOptions.ApiUrl = _config.ApiUrl;
                jsonOptions.AuthHeader = _config.AuthHeader;
            }

            _tracerProvider = TracerProvider.Create(new List<SpanProcessor>() {
                new SpanProcessor(new DebugExporter()),
                new BatchSpanProcessor(new JsonExporter(jsonOptions)),
            });

        }

        public void TogglePrivacySetting(bool set)
        {
            _jsonOptions.PrivacyOptOut = set;
        }

        public void SampleSpan()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));

            var span = tracer.GetSpan("Test Span");
            span.Attributes.Add("foo", "bar");
            span.End();
        }

        public void SampleSpanWithParent()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));

            var span = tracer.GetSpan("First_Span");
            span.Attributes.Add("foo", "bar");

            var span2 = tracer.GetSpan("Another_Span", span.SpanContext);

            span2.End();

            span.End();
        }

        public void SampleOverlappingTraces()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));
            var tracer2 = _tracerProvider.GetTracer(nameof(MetricTester));
            var span = tracer.GetSpan("Gained_Health");

            var span2 = tracer2.GetSpan("Took_Damage", span.SpanContext);
            span2.Events.Add(SpanEvent.Create("Event_Happened", Timestamp.Create()));
            span2.Attributes.Add("Damage", 5);

            span.End();
            span2.End();
        }

        public void SampleTimedTrace()
        {
            StartCoroutine(TimedTraceCoroutine());
        }

        IEnumerator TimedTraceCoroutine()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));
            var span = tracer.GetSpan("Api_Call");

            yield return new WaitForSeconds(0.1f);
            span.Events.Add(SpanEvent.Create("Some Data", Timestamp.Create()));
            yield return new WaitForSeconds(0.1f);
            span.Events.Add(SpanEvent.Create("More Data", Timestamp.Create()));
            yield return new WaitForSeconds(0.4f);
            span.Events.Add(SpanEvent.Create("Finished", Timestamp.Create()));
            span.Attributes.Add("Loaded", 2);
            span.Attributes.Add("Name", "Tester");

        }

        public void SamplePlayerConnect()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));
            var span = tracer.GetSpan("player_connect");
            span.Attributes.Add("connection", "asdfasdfadfasdf");
            span.End();
        }

        public void SamplePlayerLogin()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));
            var span = tracer.GetSpan("player_login");
            span.Attributes.Add("connection", "asdfasdfadfasdf");
            span.Attributes.Add("account_id", "123456789");
            span.End();
        }

        public void SampleCharacterAttack()
        {
            var tracer = _tracerProvider.GetTracer(nameof(MetricTester));
            var span = tracer.GetSpan("char_attack");
            span.Attributes.Add("damage", "5");
            span.Attributes.Add("char_id", "78901");
            span.End();
        }
    }
}
