﻿namespace FortifyAContainerUI.Interfaces.Container
{
    public interface ITests<T>
    {
        public abstract Task CheckForRoot(T container);
        public abstract Task CheckForDefaultNetwork(T container);
    }
}
