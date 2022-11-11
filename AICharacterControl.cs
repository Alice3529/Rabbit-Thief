using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]

    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent;
        public enemyAI character;
        public Transform target;
        Vector3 startPosition;
        Quaternion startRotation;
        Animator animator;
        [SerializeField] float radius;
        [SerializeField] Transform armPoint3;




        private void Start()
        {
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<enemyAI>();
            animator = GetComponent<Animator>();
            startPosition = transform.position;
            startRotation = transform.rotation;

            agent.updateRotation = false;
	        agent.updatePosition = true;
            Vector3 movement= agent.desiredVelocity;


        }

  
        public void SetStartState()
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            agent.enabled = false;
            target = null;
            animator.Rebind();
            animator.Update(0f);


        }



        private void Update()
        {
            if (target == null) { return; }
            if (Vector3.Distance(transform.position, target.position) > agent.stoppingDistance)
            {
                agent.SetDestination(target.position);
                character.Move(agent.velocity);
            }
            else
            {
                character.Move(Vector3.zero);
                animator.SetTrigger("kick");
             
            }
        }

        public void CollideHit()
        {
            if (FindObjectOfType<BaseGameManger>().role != 0) { return; }
            Collider[] hitColliders = Physics.OverlapSphere(armPoint3.position, radius);
            foreach (Collider col in hitColliders)
            {
                if (col.gameObject.GetComponent<player>() != null)
                {
                    col.gameObject.GetComponent<player>().CatCollide();
                }
            }
          
        }


        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(armPoint3.position, radius);

        }



        public void SetTarget(Transform target)
        {
            this.target = target;
            agent.enabled = true;

        }
    }
}
