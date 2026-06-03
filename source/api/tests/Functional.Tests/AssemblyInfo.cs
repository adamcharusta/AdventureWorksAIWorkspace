using Xunit;

// These tests each boot the application through WebApplicationFactory<Program>. With minimal
// hosting, HostFactoryResolver keeps static state while running the entry point, so building two
// hosts for the same entry point concurrently races and intermittently fails with
// "The entry point exited without ever building an IHost." Running the suite serially avoids it.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
