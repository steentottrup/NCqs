﻿using CreativeMinds.CQS.Dispatchers;
using CreativeMinds.CQS.Permissions;
using CreativeMinds.CQS.Queries;
using CreativeMinds.CQS.Validators;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CreativeMinds.CQS.Ninject {

	public class QueryDispatcher : QueryDispatcherBase {
		private readonly IReadOnlyKernel kernel;

		public QueryDispatcher(IReadOnlyKernel kernel) {
			this.kernel = kernel;
		}

		protected override IQueryHandler<TQuery, TResult> Resolve<TQuery, TResult>() {
			IQueryHandler<TQuery, TResult> handler = null;
			try {
				List<IParameter> parameters = new List<IParameter>();
				IEnumerable<Attribute> attrs = typeof(TQuery).GetTypeInfo().GetCustomAttributes();
				if (attrs.Any(a => a.GetType() == typeof(CreativeMinds.CQS.Decorators.ValidateAttribute))) {
					handler = this.kernel.Get<IGenericValidationQueryHandlerDecorator<TQuery, TResult>>();
					parameters.Add(new ConstructorArgument("wrappedHandler", handler));
				}

				if (attrs.Any(a => a.GetType() == typeof(CreativeMinds.CQS.Decorators.CheckPermissionsAttribute)) ||
					attrs.Any(a => a.GetType().GetTypeInfo().BaseType == typeof(CreativeMinds.CQS.Decorators.CheckPermissionsAttribute))) {

					handler = this.kernel.Get<IGenericPermissionCheckQueryHandlerDecorator<TQuery, TResult>>(parameters.ToArray());
				}

				if (handler == null) {
					handler = this.kernel.Get<IQueryHandler<TQuery, TResult>>();
				}
			}
			catch (Exception ex) {
				// TODO: log
				throw ex;
			}

			if (handler == null) {
				throw new RequiredHandlerNotFoundException();
			}

			return handler;
		}
	}
}