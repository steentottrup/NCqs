﻿using System;

namespace CreativeMinds.CQS.Commands {

	public interface ICommandDispatcher {
		void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
	}
}
