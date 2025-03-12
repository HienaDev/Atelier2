using UnityEngine;

public class PlayerShootingQuark : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private Transform rightArmFirePoint;
    [SerializeField] private ArmRecoil leftArmRecoil;
    [SerializeField] private ArmRecoil rightArmRecoil;

    private bool isLeftArm = true;

    [SerializeField] private float shootingCooldown = 0.15f;
    private float lastShotTime = 0f;

    [SerializeField] private float bulletSpeed = 20f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time - lastShotTime >= shootingCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private void Shoot()
    {
        // Determine which arm to use
        Transform firePoint = isLeftArm ? leftArmFirePoint : rightArmFirePoint;
        ArmRecoil recoil = isLeftArm ? leftArmRecoil : rightArmRecoil;

        // Instantiate bullet at the correct position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Set bullet velocity in the forward direction of the firePoint
        bullet.GetComponent<Rigidbody>().linearVelocity = firePoint.forward * bulletSpeed;

        // Apply recoil effect
        recoil.ApplyRecoil();

        // Alternate arms
        isLeftArm = !isLeftArm;
    }
}
