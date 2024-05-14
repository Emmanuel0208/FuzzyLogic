using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    //Requisitos del enemigo
    public Transform player;            
    public GameObject hospital;         
    public GameObject ammoBox;         
    public GameObject bulletEnemy;      
    public Transform bulletSpawn;       
    public TMP_Text enemyLife;          
    public GameObject losingPanel;      

    //Variables del enemigo
    public NavMeshAgent agent;          //Componente NavMeshAgent para la navegación
    public int maxAmmo = 8;             
    public int currentAmmo;             
    public float shootCooldown = 1.5f;  
    public float shootRange = 8f;      
    public float bulletSpeed = 10f;     
    public int maxHealth = 100;         
    public int currentHealth;          
    public bool canShoot = true;       
    public bool isReloading = false;    
    public bool isHealing = false;      
    public float distanceDifferential = 5f; 

    //Variables difusas
    private float fuzzyPlayerHealth;    //vida fuzzy
    private float fuzzyAmmo;            //Munición fuzzy
    private float fuzzyDistancePlayer;  //Distancia al jugador fuzzy
    private float fuzzyDistanceAmmo;    //Distancia a la munición fuzzy
    private float fuzzyDistanceHealth;  //Distancia a la estación de salud fuzzy

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Obtener el componente NavMeshAgent
        currentAmmo = maxAmmo;                // Inicializar munición
        currentHealth = maxHealth;            // Inicializar vida
    }

    private void Update()
    {
        if (player == null)
            return;

        Fuzzify(); //Convertir a fuzzy
        enemyLife.text = "Enemy Life: " + fuzzyPlayerHealth.ToString(); //Actualizar vida del enemigo
        Elections(); //elecciones del enemy
        if (!isReloading && !isHealing && currentAmmo > 0 && Vector3.Distance(transform.position, player.position) <= shootRange)
        {
            Shoot(); // Disparar si es posible
        }
    }

    // Convertir valores a términos difusos
    private void Fuzzify()
    {
        fuzzyPlayerHealth = (currentHealth * 100) / maxHealth;
        fuzzyAmmo = (currentAmmo * 100) / maxAmmo;
        fuzzyDistancePlayer = Vector3.Distance(transform.position, player.position);
        fuzzyDistanceAmmo = Vector3.Distance(transform.position, ammoBox.transform.position);
        fuzzyDistanceHealth = Vector3.Distance(transform.position, hospital.transform.position);
    }

    //decisiones
    private void Elections()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isNearAmmoStation = fuzzyDistanceAmmo <= 15f;
        bool isNearHealthStation = fuzzyDistanceHealth <= 15f;

        //Mantener distancia con el jugador
        if (distanceToPlayer > distanceDifferential)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        //Buscar recargar si la munición es baja y no está cerca de la estación de munición
        if (currentAmmo <= 2 && !isNearAmmoStation && distanceToPlayer > distanceDifferential)
        {
            SerchAmmo();
            return;
        }

        // Buscar curarse solo si la salud es baja y no está cerca de la estación de salud
        if (currentHealth <= 30 && !isNearHealthStation)
        {
            AidKit();
            Scape();
            return;
        }

        // Buscar curarse si está cerca de la estación de salud, solo si no se cumple la condición anterior
        if (currentHealth <= 50 && isNearHealthStation && distanceToPlayer > distanceDifferential && fuzzyDistanceHealth > 20f)
        {
            AidKit();
            Scape();
            return;
        }

        // Alejarse lo máximo posible del jugador para buscar munición solo si no se cumple ninguna de las condiciones anteriores
        if (currentAmmo <= maxAmmo / 2 && !isNearHealthStation && distanceToPlayer > distanceDifferential)
        {
            Scape();
            return;
        }

        // Alejarse lo máximo posible del jugador si la munición y la vida están al máximo, solo si no se cumple ninguna de las condiciones anteriores
        if (currentAmmo == maxAmmo && currentHealth == maxHealth && distanceToPlayer > distanceDifferential)
        {
            Scape();
            return;
        }
    }

    // Escapar del jugador
    private void Scape()
    {
        Vector3 scapeDirection = transform.position - player.position;
        scapeDirection.y = 0;
        scapeDirection.Normalize();
        Vector3 scapeZone = transform.position - player.position;
        agent.SetDestination(scapeZone);
    }

    // Disparar al jugador
    private void Shoot()
    {
        if (canShoot && currentAmmo > 0)
        {
            GameObject bullet = Instantiate(bulletEnemy, bulletSpawn.position, Quaternion.identity);

            Vector3 direction = (player.position - bulletSpawn.position).normalized;

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = direction * bulletSpeed;
            }

            Destroy(bullet, 3f);

            currentAmmo--;

            StartCoroutine(ShootCooldown());
        }
    }

    // Moverse a la estación de salud
    private void AidKit()
    {
        agent.SetDestination(hospital.transform.position);
        if (Vector3.Distance(transform.position, hospital.transform.position) < 1f)
        {
            StartCoroutine(HealOverTime());
        }
    }

    // Moverse a la estación de munición
    private void SerchAmmo()
    {
        agent.SetDestination(ammoBox.transform.position);
    }

    // Recargar munición
    private void RestockAmmo()
    {
        currentAmmo = maxAmmo;
    }

    // Curar al enemigo
    public void Heal()
    {
        currentHealth = maxHealth;
    }

    // Cooldown entre disparos
    private IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    // Recargar munición con un retraso
    private IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(shootCooldown);

        RestockAmmo();

        isReloading = false;

        agent.isStopped = false;
    }

    // Método para manejar la muerte del enemigo
    private void Die()
    {
        Time.timeScale = 0f;
        losingPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    // Reiniciar el juego
    public void Restart()
    {
        StartCoroutine(RestartCoroutine());
    }

    // Corutina para reiniciar el juego
    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Manejar colisiones con otros objetos
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ammoBox && !isReloading)
        {
            agent.isStopped = true;
            StartCoroutine(Reload());
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(20);
            Destroy(other.gameObject);
        }
    }

    // Tomar daño
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Curar al enemigo con el tiempo
    private IEnumerator HealOverTime()
    {
        isHealing = true;
        while (currentHealth < maxHealth)
        {
            yield return new WaitForSeconds(1f);
            currentHealth += 20;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        isHealing = false;
    }

    // Cargar el menú inicial
    public void InitialMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    // Salir del juego
    public void Quit()
    {
        Application.Quit();
    }
}
