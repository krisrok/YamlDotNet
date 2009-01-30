using System;
using System.Globalization;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents an alias event.
	/// </summary>
	public class AnchorAlias : ParsingEvent, IAnchorAlias
	{
		/// <summary>
		/// Gets a value indicating the variation of depth caused by this event.
		/// The value can be either -1, 0 or 1. For start events, it will be 1,
		/// for end events, it will be -1, and for the remaining events, it will be 0.
		/// </summary>
		public override int NestingIncrease {
			get {
				return 0;
			}
		}

		/// <summary>
		/// Gets the event type, which allows for simpler type comparisons.
		/// </summary>
		internal override EventType Type {
			get {
				return EventType.YAML_ALIAS_EVENT;
			}
		}
		
		private readonly string value;

		/// <summary>
		/// Gets the value of the alias.
		/// </summary>
		public string Value
		{
			get
			{
				return value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorAlias"/> class.
		/// </summary>
		/// <param name="value">The value of the alias.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public AnchorAlias(string value, Mark start, Mark end)
			: base(start, end)
		{
			if(string.IsNullOrEmpty(value)) {
				throw new YamlException("Anchor value must not be empty.");
			}

			if(!NodeEvent.anchorValidator.IsMatch(value)) {
				throw new YamlException("Anchor value must contain alphanumerical characters only.");
			}
			
			this.value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorAlias"/> class.
		/// </summary>
		/// <param name="value">The value of the alias.</param>
		public AnchorAlias(string value)
			: this(value, Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Alias [value = {0}]", value);
		}
	}
}
