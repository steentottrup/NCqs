﻿using CreativeMinds.CQS.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CreativeMinds.CQS.Validators {

	public class GenericValidationAsyncCommandHandlerDecorator<TCommand> : IGenericValidationAsyncCommandHandlerDecorator<TCommand> where TCommand : ICommand {
		private readonly IAsyncCommandHandler<TCommand> wrappedHandler;
		private readonly IEnumerable<IAsyncValidator<TCommand>> validators;
		protected readonly ILogger logger;

		public GenericValidationAsyncCommandHandlerDecorator(IAsyncCommandHandler<TCommand> wrappedHandler, IEnumerable<IAsyncValidator<TCommand>> validators, ILogger<GenericValidationAsyncCommandHandlerDecorator<TCommand>> logger) {
			this.wrappedHandler = wrappedHandler ?? throw new ArgumentNullException(nameof(wrappedHandler));
			this.validators = validators ?? throw new ArgumentNullException(nameof(validators));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task ExecuteAsync(TCommand command, CancellationToken cancellationToken) {
			if (this.validators.Any()) {
				this.logger.LogInformation("Command handler validations found", this.validators);
				List<ValidationResult> results = new List<ValidationResult>();
				foreach (var validator in this.validators) {
					ValidationResult result = await validator.ValidateAsync(command, cancellationToken);
					results.Add(result);
				}

				if (results.Any(r => r.Errors.Any())) {
					this.logger.LogCritical("Command handler validations returned errors", results);
					throw new ValidationException(results.SelectMany(r => r.Errors));
				}
			}
			else {
				this.logger.LogWarning($"A validation decorator was found, but no validations for \"{typeof(TCommand).Name}\" command");
			}
			await this.wrappedHandler.ExecuteAsync(command, cancellationToken);
		}
	}
}
