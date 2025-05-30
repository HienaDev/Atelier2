using UnityEngine;

public class PlayerShootingMonkeyHell : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ArmRecoil leftArmRecoil;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private ArmRecoil rightArmRecoil;
    [SerializeField] private Transform rightArmFirePoint;
    [SerializeField] private float shootingCooldown = 0.15f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private GameObject topHalf;
    [SerializeField] private float rotationSpeed = 10f;

    private bool isLeftArm = true;
    private float justShot = 0f;

    private PlayerSounds playerSounds;

    private void Start()
    {
        playerSounds = GetComponent<PlayerSounds>();
        
    }


    void Update()
    {
        // Get direction from object to the hit point, but only on the X-Z plane
        Vector3 direction = Mouse3D.GetMouseObjectPosition() - topHalf.transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            // Create target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target
            topHalf.transform.rotation = Quaternion.Slerp(topHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (Input.GetButton("Fire1") && Time.time - justShot >= shootingCooldown)
        {
            Shoot();
            justShot = Time.time;
        }
    }

    public void Shoot()
    {
        playerSounds.PlayerShoot();
        if (isLeftArm)
        {
            GameObject bullet = Instantiate(bulletPrefab, leftArmFirePoint.position, leftArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            leftArmRecoil.ApplyRecoil();
            isLeftArm = false;
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab, rightArmFirePoint.position, rightArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            rightArmRecoil.ApplyRecoil();
            isLeftArm = true;
        }
    }
}
