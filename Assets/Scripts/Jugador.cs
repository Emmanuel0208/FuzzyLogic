using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Jugador : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 5f;
    public int maxHealth = 100;
    public int currentHealth;

    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 10f;
    public int maxAmmo = 8;
    private int currentAmmo;
    public float shootCooldown = 1.5f;
    private bool canShoot = true;

    //die
    public GameObject endPanel;

    //life
    public TMP_Text textoVida;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        Move();
        textoVida.text = "Your life: " + currentHealth.ToString();
        Shoot();
        Reload();
    }

    private void Move()
    {
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(hor, 0, ver) * moveSpeed * Time.deltaTime;

        rb.MovePosition(transform.position + movement);
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && canShoot && currentAmmo > 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody != null)
            {
                Vector3 direction = (shootPoint.forward).normalized;
                bulletRigidbody.velocity = direction * bulletSpeed;
            }
            Destroy(bullet, 3f);
            currentAmmo--;
            StartCoroutine(ShootCooldown());
        }
    }

    private void Reload()
    {
        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private System.Collections.IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    private System.Collections.IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(3.5f);
        currentAmmo = maxAmmo;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Time.timeScale = 0f;
        endPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    //panel
    public void Restart()
    {
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void InitialMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(20);
            Destroy(other.gameObject);
        }
    }
}