using UnityEngine;

/*namespace ProcMeshes 
{*/
	// Base generator interface
	public interface IMeshGenerator 
	{
		// Getters for each values needed to produce the varying mesh types that are defined in the 
		// inherited generators
		Bounds Bounds { get; }

		int VertexCount { get; }

		int IndexCount { get; }

		int JobLength { get; }

		int Resolution { get; set; }

		// Executed by the MeshJob struct with an index parameter and an IMeshStreams struct used for 
		// storage. 
		void Execute<S> (int i, S streams) where S : struct, IMeshStreams;
	}
