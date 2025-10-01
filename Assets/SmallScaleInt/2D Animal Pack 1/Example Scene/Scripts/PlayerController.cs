using UnityEngine;
using System.Collections;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 2.0f; // the movement speed of the player
        private Rigidbody2D rb;
        private Vector2 movementDirection;
        private bool isOnStairs = false; // when on stairs, the player moves in a different angle.
        public bool isCrouching = false; // when crouching, the player moves slower
        private SpriteRenderer spriteRenderer;
        private float lastAngle;  // Store the last calculated angle
        private bool isRunning = false;

        // Archer specifics
        public bool isActive; // If the character is active
        public bool isRanged; // If the character is an archer OR caster character
        public bool isStealth; // If true, Makes the player transparent when crouched

        public bool isShapeShifter; // If true, Makes the player transparent when crouched
        public bool isSummoner; // If true, Makes the player transparent when crouched
        public GameObject projectilePrefab; // prefab to the projectile
        public GameObject AoEPrefab;
        public GameObject Special1Prefab;
        public GameObject HookPrefab; // Certain characters might have a grappling hook

        public GameObject ShapeShiftPrefab; // Certain characters might have a grappling hook

        public float projectileSpeed = 10.0f; // Speed at which the projectile travels
        public float shootDelay = 0.5f; // Delay in seconds before the projectile is fired

        // Melee specifics
        public bool isMelee; // If the character is a melee character
        public GameObject meleePrefab; // prefab for the melee attack



        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component

        }

        void Update()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            lastAngle = SnapAngleToEightDirections(angle);  // Update lastAngle here

            movementDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));

            HandleMovement();

            // Check if movement keys are pressed
            bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

            if (isMoving && !isRunning)
            {
                isRunning = true;
            }
            else if (!isMoving && isRunning)
            {
                isRunning = false;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if(isShapeShifter && isActive)
                {
                    StartCoroutine(ShapeShiftDelayed());
                }
                HandleCrouching();
            }

            if(isActive)
            {
                //IF any prefab is missing, dont run any projectile code.. (the below line can be removed if you want to use projectiles.)
                if (projectilePrefab == null || AoEPrefab == null || Special1Prefab == null || HookPrefab == null)
                {
                    return;
                }

                if (isRanged)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);

                    }

                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeploySpecial1Delayed());
                    }
              
                    if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        StartCoroutine(DeployAoEDelayed());
                    }    

                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        if(isSummoner)
                        {StartCoroutine(DeployHookDelayed());}
                        else
                        {StartCoroutine(Quickshot());}
                        
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        StartCoroutine(CircleShot());
                        
                    }                                    
                }

                if (isMelee)
                {

                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeployAoEDelayed());

                    }                                    
                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        StartCoroutine(DeployHookDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);
                    }
                }

                else if (Input.GetKeyDown(KeyCode.LeftControl) && isRunning)
                {
                    if(isShapeShifter && isActive)
                    {
                        StartCoroutine(ShapeShiftDelayed());
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (movementDirection != Vector2.zero)
            {
                rb.MovePosition(rb.position + movementDirection * speed * Time.fixedDeltaTime);
            }
        }


    

        float SnapAngleToEightDirections(float angle)
        {
            angle = (angle + 360) % 360;

            if (isOnStairs)
            {
                // Angle adjustments when on stairs
                if (angle < 30 || angle >= 330)
                    return 0;
                else if (angle >= 30 && angle < 75)
                    return 60;
                else if (angle >= 75 && angle < 105)
                    return 90;
                else if (angle >= 105 && angle < 150)
                    return 120;
                else if (angle >= 150 && angle < 210)
                    return 180;
                else if (angle >= 210 && angle < 255)
                    return 240;
                else if (angle >= 255 && angle < 285)
                    return 270;
                else if (angle >= 285 && angle < 330)
                    return 300;
            }
            else
            {
                // Normal angle adjustments
                if (angle < 15 || angle >= 345)
                    return 0; // East (isEast)
                else if (angle >= 15 && angle < 60)
                    return 35; // Northeast (isNorthEast)
                else if (angle >= 60 && angle < 120)
                    return 90; // North (isNorth)
                else if (angle >= 120 && angle < 165)
                    return 145; // Northwest (isNorthWest)
                else if (angle >= 165 && angle < 195)
                    return 180; // West (isWest)
                else if (angle >= 195 && angle < 240)
                    return 215; // Southwest (isSouthWest)
                else if (angle >= 240 && angle < 300)
                    return 270; // South (isSouth)
                else if (angle >= 300 && angle < 345)
                    return 330; // Southeast (isSouthEast)

            }

            return 0;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = false;
            }
        }

        float GetPerpendicularAngle(float angle, bool isLeft)
        {
            // Calculate the base perpendicular angle (90 degrees offset)
            float perpendicularAngle = isLeft ? angle - 90 : angle + 90;
            perpendicularAngle = (perpendicularAngle + 360) % 360; // Normalize the angle

            // Use your SnapAngleToEightDirections function to snap to the nearest valid angle
            return SnapAngleToEightDirections(perpendicularAngle);
        }

        void HandleMovement()
        {
            if (Input.GetKey(KeyCode.W))
            {
                return;
            }
            else if (!isCrouching) // Allow strafing only when not crouching, if desired
            {
                if (Input.GetKey(KeyCode.S))
                {
                    movementDirection = -movementDirection; // Move backwards
                }

                else if (Input.GetKey(KeyCode.A))
                {
                    float leftAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, true);
                    movementDirection = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));

                }
                else if (Input.GetKey(KeyCode.D))
                {

                    float rightAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, false);
                    movementDirection = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));
                }
                else
                {
                    movementDirection = Vector2.zero; // No movement input
                }
            }
            else
            {
                movementDirection = Vector2.zero; // No movement input
            }
        }

        void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching; // Toggle crouching
                // speed = isCrouching ? 1.0f : 2.0f; // Adjust speed based on crouch state if needed

                if (isCrouching && isStealth)
                {
                    // Set the color to dark gray and reduce opacity to 50%
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    // Reset the color to white and opacity to 100%
                    spriteRenderer.color = Color.white;
                }
            }
        }

        //Ranged character specific methods:

        public void SetArcherStatus(bool status)
        {
            isRanged = status;
        }

        public void SetActiveStatus(bool status)
        {
            isActive = status;
        }

        void DelayedShoot()
        {
            Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
            ShootProjectile(fireDirection);
        }

        void ShootProjectile(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Rigidbody2D rbProjectile = projectileInstance.GetComponent<Rigidbody2D>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = direction * projectileSpeed;
            }
            // Destroy the instantiated prefab after another 1.5 seconds
            Destroy(projectileInstance, 1.5f);
        }

        IEnumerator Quickshot()
        {
            // Initial small delay before starting the quickshot sequence
            yield return new WaitForSeconds(0.1f);

            // Loop to fire five projectiles in the facing direction
            for (int i = 0; i < 5; i++)
            {
                Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                ShootProjectile(fireDirection);

                // Wait for 0.18 seconds before firing the next projectile
                yield return new WaitForSeconds(0.18f);
            }
        }

        IEnumerator CircleShot()
        {
            float initialDelay = 0.1f;
            float timeBetweenShots = 0.9f / 8;  // Total time divided by the number of shots

            yield return new WaitForSeconds(initialDelay);

            // Use the lastAngle as the start angle and generate projectiles in 8 directions
            for (int i = 0; i < 8; i++)
            {
                float angle = lastAngle + i * 45;  // Increment by 45 degrees for each direction
                angle = Mathf.Deg2Rad * angle;  // Convert to radians for direction calculation
                Vector2 fireDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ShootProjectile(fireDirection);

                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        IEnumerator DeployAoEDelayed()
        {
            if (AoEPrefab != null)
            {
                GameObject aoeInstance; // Declare outside to ensure visibility for later destruction

                if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.3f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the AoE prefab at the mouse's world position
                    aoeInstance = Instantiate(AoEPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(aoeInstance, 8.7f);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else if(isShapeShifter)
                    {
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                    // Instantiate the AoE prefab at the player's position
                    aoeInstance = Instantiate(AoEPrefab, transform.position, Quaternion.identity);
                    Destroy(aoeInstance, 0.9f);
                }

                // Destroy the AoE instance after 0.9 seconds
                
            }
        }


        IEnumerator ShapeShiftDelayed()
        {
            if (ShapeShiftPrefab != null)
            {

                yield return new WaitForSeconds(0.001f);
                
                // Instantiate the AoE prefab at the player's position
                GameObject shapeShiftInstance = Instantiate(ShapeShiftPrefab, transform.position, Quaternion.identity);

                
                // Destroy the instantiated prefab after another 0.5 seconds
                Destroy(shapeShiftInstance, 0.9f);
            }
        }
        IEnumerator DeploySpecial1Delayed()
        {
            if (Special1Prefab != null)
            {
                GameObject Special1PrefabInstance; // Declare outside to ensure visibility for later destruction

                if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the Special1 prefab at the mouse's world position
                    Special1PrefabInstance = Instantiate(Special1Prefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.6f);
                    }
                    // Instantiate the Special1 prefab at the player's position
                    Special1PrefabInstance = Instantiate(Special1Prefab, transform.position, Quaternion.identity);
                }

                // Destroy the Special1 instance after 1.0 seconds
                Destroy(Special1PrefabInstance, 1.0f);
            }
        }

        IEnumerator DeployHookDelayed()
        {
            GameObject hookInstance;
            if (isSummoner)
                {
                    // Get mouse position and convert it to world coordinates
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // Set this to your camera's near clip plane
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // Wait before instantiating (adjust time as needed)
                    // Instantiate the Special1 prefab at the mouse's world position
                    hookInstance = Instantiate(HookPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(hookInstance, 5.2f);
                }
                else
                {
                    if (HookPrefab != null)
                    {
                        Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        hookInstance = Instantiate(HookPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                        // Destroy the instantiated prefab after another 1.0 seconds
                        Destroy(hookInstance, 1.0f);
                    }
                    yield return null; // Ensures the method correctly implements IEnumerator
                }
        }


        // Melee attack method
        // void MeleeAttack()
        // {
        //     if (meleePrefab != null)
        //     {
        //         StartCoroutine(DelayedMeleeAttack());
        //     }
        // }

        // IEnumerator DelayedMeleeAttack()
        // {
        //     // Wait for 0.5 seconds before initiating the melee attack
        //     yield return new WaitForSeconds(0.5f);

        //     Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
        //     // Calculate the rotation angle for the melee attack
        //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //     // Instantiate the melee attack prefab at the player's position
        //     GameObject meleeInstance = Instantiate(meleePrefab, transform.position, Quaternion.Euler(0, 0, angle));

        //     // Set the instantiated melee attack prefab as a child of the player
        //     meleeInstance.transform.SetParent(transform);

        //     // Optionally, destroy the melee attack prefab after a short duration
        //     Destroy(meleeInstance, 0.1f); // Adjust the duration as needed
        // }

    }
}
