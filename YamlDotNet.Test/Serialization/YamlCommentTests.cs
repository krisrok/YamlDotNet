﻿using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace YamlDotNet.Test.Serialization
{
    public class YamlCommentTests
    {
        protected readonly ITestOutputHelper Output;
        public YamlCommentTests(ITestOutputHelper helper)
        {
            Output = helper;
        }

        #region Simple block comments
        [Fact]
        public void SerializationWithBlockComments()
        {
            var person = new Person { Name = "PandaTea", Age = 100 };

            var serializer = new Serializer();
            var result = serializer.Serialize(person);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Person>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines.Should().Contain("# The person's name");
            lines.Should().Contain("# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_Multiline()
        {
            var person = new Car();

            var serializer = new Serializer();
            var result = serializer.Serialize(person);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Car>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines.Should().Contain("# The car's rightful owner");
            lines.Should().Contain("# or:");
            lines.Should().Contain("# This person owns the car");
        }

        [Fact]
        public void SerializationWithBlockComments_NullValue()
        {
            var serializer = new Serializer();
            Action action = () => serializer.Serialize(new NullComment());
            action.ShouldNotThrow();
        }
        #endregion

        #region Indentation of block comments
        [Fact]
        public void SerializationWithBlockComments_IndentedInSequence()
        {
            var person = new Person { Name = "PandaTea", Age = 100 };

            var serializer = new Serializer();
            var result = serializer.Serialize(new Person[] { person });
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Person[]>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent = GetIndent(1);

            lines.Should().Contain("- # The person's name");
            lines.Should().Contain(indent + "# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_IndentedInBlock()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Owner = new Person { Name = "PandaTea", Age = 100 }
                }
            };

            var serializer = new Serializer();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);

            lines.Should().Contain(indent1 + "# The car's rightful owner");
            lines.Should().Contain(indent1 + "# or:");
            lines.Should().Contain(indent1 + "# This person owns the car");
            lines.Should().Contain(indent2 + "# The person's name");
            lines.Should().Contain(indent2 + "# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_IndentedInBlockAndSequence()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Passengers = new[]
                    {
                        new Person { Name = "PandaTea", Age = 100 }
                    }
                }
            };

            var serializer = new Serializer();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);

            lines.Should().Contain(indent1 + "# The car's rightful owner");
            lines.Should().Contain(indent1 + "- # The person's name");
            lines.Should().Contain(indent2 + "# The person's age");
        }
        #endregion

        #region Flow mapping
        [Fact]
        public void SerializationWithBlockComments_IndentedInBlockAndSequence_WithFlowMapping()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Owner = new Person { Name = "Paul", Age = 50 },
                    Passengers = new[]
                    {
                        new Person { Name = "PandaTea", Age = 100 }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithEventEmitter(e => new FlowEmitter(e, typeof(Person)))
                .Build();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);

            lines.Should().Contain("# The car parked in the garage");
            lines.Should().Contain(indent1 + "# The car's rightful owner");
            result.Should().NotContain("The person's name", "because the person's properties are inside of a flow map now");
            result.Should().NotContain("The person's age", "because the person's properties are inside of a flow map now");
        }

        /// <summary>
        /// This emits objects of given types as flow mappings
        /// </summary>
        public class FlowEmitter : ChainedEventEmitter
        {
            private readonly Type[] types;

            public FlowEmitter(IEventEmitter nextEmitter, params Type[] types) : base(nextEmitter)
            {
                this.types = types;
            }

            public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
            {
                foreach (var type in types)
                {
                    if (eventInfo.Source.Type == type)
                    {
                        eventInfo.Style = MappingStyle.Flow;
                        break;
                    }
                }
                base.Emit(eventInfo, emitter);
            }
        }
        #endregion

        #region Inline comments
//        [Fact]
//        public void SFlowStyle_EmptyChild()
//        {
//            var yaml = @"
//Foo: 0
//Child: {Foo: 0, Child: { Child: , Foo: 0 }}";

//            var deserializer = new Deserializer();
//            Action action = () => deserializer.Deserialize<InlineComments>(yaml);
//            action.ShouldNotThrow();
//        }

        [Fact]
        public void SerializationWithInlineComment_WithReference()
        {
            var serializer = new SerializerBuilder()
                .WithEmissionPhaseObjectGraphVisitor(e => new CustomCommentsObjectGraphVisitor(e.InnerVisitor), p => p.OnTop())
                .Build();
            var result = serializer.Serialize(new InlineComments { Child = new InlineComments() });
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<InlineComments>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);

            lines.Should().Contain("Child: # Child: Inline for reference type");
            lines.Should().Contain(indent1 + "Foo: 0 # Foo: Inline and single-line");
        }

        [Fact]
        public void SerializationWithInlineComment_WithSequence()
        {
            var serializer = new SerializerBuilder()
                .WithEmissionPhaseObjectGraphVisitor(e => new CustomCommentsObjectGraphVisitor(e.InnerVisitor), p => p.OnTop())
                .Build();
            var result = serializer.Serialize(new InlineComments { Children = new[] { new InlineComments() } });
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<InlineComments>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);

            lines.Should().Contain("Children: # Children: Inline for sequence type");
            lines.Should().Contain(indent1 + "Foo: 0 # Foo: Inline and single-line");
        }

        [Fact]
        public void SerializationWithInlineComment_WithEmptyReferenceAndEmptySequence()
        {
            var serializer = new SerializerBuilder()
                .WithEmissionPhaseObjectGraphVisitor(e => new CustomCommentsObjectGraphVisitor(e.InnerVisitor), p => p.OnBottom())
                .Build();
            var result = serializer.Serialize(new InlineComments());
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<InlineComments>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines.Should().Contain("Foo: 0 # Foo: Inline and single-line");
            lines.Should().Contain("Bar: 00:00:00 # Bar: Inline and concatenated multi-line");
            lines.Should().Contain("Child: # Child: Inline for reference type");
            lines.Should().Contain("Children: # Children: Inline for sequence type");
        }

        class InlineComments
        {
            [CustomComment("Foo: Inline and single-line", true)]
            public int Foo { get; set; }

            [CustomComment("Bar: Inline\nand concatenated\nmulti-line", true)]
            public TimeSpan Bar { get; set; }

            [CustomComment("Child: Inline for reference type", true)]
            public InlineComments Child { get; set; }

            [CustomComment("Children: Inline for sequence type", true)]
            public InlineComments[] Children { get; set; }
        }

        class InlineCommentsChild : InlineComments
        { }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
        sealed class CustomCommentAttribute : Attribute
        {
            public string Description { get; private set; }
            public bool IsInline { get; private set; }

            public CustomCommentAttribute(string description, bool isInline = false)
            {
                Description = description;
                IsInline = isInline;
            }
        }

        public sealed class CustomCommentsObjectGraphVisitor : ChainedObjectGraphVisitor
        {
            public CustomCommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
                : base(nextVisitor)
            { }

            readonly Stack<IObjectDescriptor> waitingObjectDescriptors = new Stack<IObjectDescriptor>();
            readonly Stack<CustomCommentAttribute> waitingAttributes = new Stack<CustomCommentAttribute>();

            public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
            {
                if (waitingObjectDescriptors.Count > 0)
                {
                    waitingObjectDescriptors.Pop();
                    var cc = waitingAttributes.Pop();
                    context.Emit(new Comment(cc.Description, true));
                }

                var result = base.EnterMapping(key, value, context);

                var customComment = key.GetCustomAttribute<CustomCommentAttribute>();
                if (customComment?.Description != null)
                {
                    if (customComment.IsInline && IsScalar(value))
                    { }
                    else if (customComment.IsInline && IsScalar(value) == false)
                    {
                        waitingObjectDescriptors.Push(value);
                        waitingAttributes.Push(customComment);
                    }
                    else if (customComment.IsInline == false)
                    {
                        context.Emit(new Comment(customComment.Description, false));
                    }
                }

                return result;
            }

            //public override void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, IEmitter context)
            //{
            //    if (waitingObjectDescriptors.Count > 0)
            //    {
            //        waitingObjectDescriptors.Pop();
            //        var cc = waitingAttributes.Pop();
            //        context.Emit(new Comment(cc.Description, true));
            //    }

            //    base.VisitSequenceStart(sequence, elementType, context);
            //}

            public override void VisitMappingEnd(IObjectDescriptor mapping, IEmitter context)
            {
                if (waitingObjectDescriptors.Count > 0)
                {
                    waitingObjectDescriptors.Pop();
                    var cc = waitingAttributes.Pop();
                    context.Emit(new Comment(cc.Description, true));
                }

                base.VisitMappingEnd(mapping, context);
            }

            public override void VisitAfterValue(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
            {
                base.VisitAfterValue(key, value, context);

                if (IsScalar(value))
                {
                    var customComment = key.GetCustomAttribute<CustomCommentAttribute>();
                    if (customComment?.Description != null)
                    {
                        if (customComment.IsInline)
                        {
                            context.Emit(new Comment(customComment.Description, true));
                        }
                    }
                }
            }

            private static bool IsScalar(IObjectDescriptor value)
            {
                var typeCode = value.Type.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.String:
                    case TypeCode.Char:
                    case TypeCode.DateTime:
                        return true;
                }

                return value.Value == null || value.Type == typeof(TimeSpan);
            }
        }
        #endregion

        class Person
        {
            [YamlMember(Description = "The person's name")]
            public string Name { get; set; }
            [YamlMember(Description = "The person's age")]
            public int Age { get; set; }
        }

        class Car
        {
            [YamlMember(Description = "The car's rightful owner\nor:\nThis person owns the car")]
            public Person Owner { get; set; }
            public Person[] Passengers { get; set; }
        }

        class Garage
        {
            [YamlMember(Description = "The car parked in the garage")]
            public Car Car;
        }

        class NullComment
        {
            [YamlMember(Description = null)]
            public int Foo { get; set; }
        }

        private static string[] SplitByLines(string result)
        {
            return result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        private static string GetIndent(int depth)
        {
            var indentWidth = EmitterSettings.Default.BestIndent;
            var indent = "";
            while (indent.Length < indentWidth * depth)
            {
                indent += " ";
            }

            return indent;
        }
    }
}
