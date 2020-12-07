using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace manipulacionBD
{
    public partial class Form1 : Form
    {
        MySqlConnection conexionBD;
        MySqlDataAdapter adapt;
        MySqlCommand cmd;
        MySqlCommand comandovalidacion;
        MySqlDataReader reader = null;
        public bool vacio;
        private void ValidarVacios(Form Form1)
        {
            foreach (Control control in Form1.Controls)
            {
                if (control is TextBox & control.Text == String.Empty)
                {
                    vacio = true;   
                }
            }

            if (vacio) MessageBox.Show("LLene todos los campos");
            vacio = false;
            
        }
        public void verificarBD()//Valida la conexión de la base de datos
        {
            string cadenaconexion = "server=localhost; port=3306; user id=root; password=admin99; database=videojuegosFrag;";//En caso marque error inserter la contraseña y la base de datos tiene que ser la que se quiera usar
            conexionBD = new MySqlConnection(cadenaconexion);

            try
            {
                conexionBD.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conexionBD.Close();
        }

        public void DisplayData()//Junta los fragmentos y los muestra en el datagrid
        {
            conexionBD.Open();
            DataTable dt = new DataTable();
            adapt = new MySqlDataAdapter("select * from sucursales1 union select * from sucursales2 union select * from sucursales3", conexionBD);
            adapt.Fill(dt);
            dataGridView1.DataSource = dt;
            conexionBD.Close();
        }
        private void ClearData()//Limpia los campos xd
        {
            maskedcvesucursal.Text = "";
            txt_Zona.Text = "";
            txt_direccion.Text  = "";
            txt_email.Text = "";
            txt_telefono.Text = "";
            
        }

        public Form1()//El constructor del FORM o la aplicación.
        {
            InitializeComponent();
            verificarBD();
            DisplayData();
            ClearData();            
        }

        public string getFragmento(String zona)//Funcion que devuelve el fragmento
        {
            String fragmento = "";
            cmd = new MySqlCommand("select fragmento from catalogo where valor = @zona", conexionBD);
            cmd.Parameters.AddWithValue("@zona", zona);
            conexionBD.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                fragmento = reader.GetString(0);
            }           
            conexionBD.Close();
            return fragmento;
        }
        public Boolean validacionZona(String zona)//Funcion que devuelve si la zona que se está ingresando cuenta con la llave primaria.
        {
            Boolean check = true;
            String fragmento = "";
            cmd = new MySqlCommand("select fragmento from catalogo where valor = @zona", conexionBD);
            cmd.Parameters.AddWithValue("@zona", zona);
            conexionBD.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                fragmento = reader.GetString(0);
            }
            conexionBD.Close();
            cmd = new MySqlCommand("select cvesucursal from "+ fragmento, conexionBD);
            conexionBD.Open();
            reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                if (reader.GetString(0)==maskedcvesucursal.Text)
                {
                     check = true;
                }
                else
                {
                     check = false;
                }
            }
            conexionBD.Close();
            return check;
        }

        public Boolean validarCampos(String cvesucursal, String zona, String direccion, int telefono, String email,String Metodo)//Método que nos sirve para validar los campos
        {
            Boolean check = true;
            Boolean deleteChecker = false;
            comandovalidacion = new MySqlCommand("select cvesucursal from sucursales1 union select cvesucursal from sucursales2 union select cvesucursal from sucursales3;", conexionBD);
            conexionBD.Open();
            reader = comandovalidacion.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(0) == cvesucursal)
                {
                    if (Metodo != "borrar")
                    {
                        check = false;
                        MessageBox.Show("El cvesucursal ya existe, ingrese uno que no se repita");
                        maskedcvesucursal.Text = "";

                    }
                    else 
                    {
                        deleteChecker = true;
                    }
                    
                }
            }
            if (deleteChecker == false && Metodo == "borrar")
            {
                check = false;
                MessageBox.Show("El valor no existe. Ingrese uno que ya exista.");
                maskedcvesucursal.Text = "";
            }
            conexionBD.Close();

            if (zona != "Aguascalientes" && zona != "CDMX" && zona != "Monterrey")
            {
                check = false;
                MessageBox.Show("Ingrese una zona valida");
                txt_Zona.Text = "";
            }

            return check;
        }
        private void botonInsertar_Click(object sender, EventArgs e)//Método del botón insertar
        {
            if (maskedcvesucursal.Text != null && txt_Zona.Text != "" && txt_direccion.Text != "" && txt_telefono.Text != "" && txt_email.Text != "")
            {
                if (validarCampos(maskedcvesucursal.Text, txt_Zona.Text, txt_direccion.Text, Convert.ToInt32(txt_telefono.Text), txt_email.Text, "insertar"))
                {
                    String Fragmento = getFragmento(txt_Zona.Text);
                    DialogResult confEliminacion = MessageBox.Show("El registro " + maskedcvesucursal.Text + " será insertado", "Advertencia", MessageBoxButtons.YesNo);
                    if (confEliminacion == DialogResult.Yes)
                    {
                        cmd = new MySqlCommand("insert into " + Fragmento + " values(@id, @zona, @direccion, @telefono, @email)", conexionBD);
                        conexionBD.Open();
                        cmd.Parameters.AddWithValue("@id", Convert.ToInt32(maskedcvesucursal.Text));

                        cmd.Parameters.AddWithValue("@zona", txt_Zona.Text);
                        cmd.Parameters.AddWithValue("@direccion", txt_direccion.Text);
                        cmd.Parameters.AddWithValue("@telefono", Convert.ToInt32(txt_telefono.Text));
                        cmd.Parameters.AddWithValue("@email", txt_email.Text);
                        cmd.ExecuteNonQuery();
                        conexionBD.Close();
                        MessageBox.Show("Insertado con éxito");
                        DisplayData();
                        ClearData();
                    }
                    else
                    {
                        MessageBox.Show("Operación Cacelada");
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("¡Porfavor! Inserte valores");
            }
        }
        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)//Método que funciona como envento para llenar los campos con los valores del DataGrid
        {
            maskedcvesucursal.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            txt_Zona.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            txt_direccion.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
            txt_telefono.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
            txt_email.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
        }

        private void botonBorrar_Click(object sender, EventArgs e)//Procesos del botón Borrar
        {
            if (maskedcvesucursal.Text != null && txt_Zona.Text != "" && txt_direccion.Text != "" && txt_telefono.Text != "" && txt_email.Text != "")
            {
                if (validarCampos(maskedcvesucursal.Text, txt_Zona.Text, txt_direccion.Text, Convert.ToInt32(txt_telefono.Text), txt_email.Text,"borrar"))
                {
                    if (validacionZona(txt_Zona.Text))
                    {
                        String Fragmento = getFragmento(txt_Zona.Text);
                        DialogResult confEliminacion = MessageBox.Show("El registro " + maskedcvesucursal.Text + " será eliminado", "Advertencia", MessageBoxButtons.YesNo);
                        if(confEliminacion == DialogResult.Yes)
                        {
                            cmd = new MySqlCommand("DELETE FROM " + Fragmento + " WHERE cvesucursal = @id", conexionBD);
                            conexionBD.Open();
                            cmd.Parameters.AddWithValue("@id", Convert.ToInt32(maskedcvesucursal.Text));

                            cmd.ExecuteNonQuery();
                            conexionBD.Close();
                            MessageBox.Show("Se ha eliminado con éxito");
                            DisplayData();
                            ClearData();
                        }
                        else
                        {
                            MessageBox.Show("Operación Cacelada");
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Al momento de elminar ingrese la zona correctamente.");
                        txt_Zona.Text = "";
                    }

                }
            }
            else
            {
                MessageBox.Show("¡Porfavor! Inserte valores");
            }
        }

        private void button_Cambio_Click(object sender, EventArgs e)//Procesos del botón Cambio
        {

        }

        private void button_Administracion_Click(object sender, EventArgs e)
        {
            Form admin = new adminE();
            admin.Show();
        }

        private void button_Consultas_Click(object sender, EventArgs e)
        {
            Form consultas = new consultasE();
            consultas.Show();
        }

        private void txtcvsucursal_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void txt_email_Validating(object sender, CancelEventArgs e)
        {
            System.Text.RegularExpressions.Regex  email = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z][\w\.-]{2,28}[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");
            if (txt_email.Text.Length>0) 
            {
                if (!email.IsMatch(txt_email.Text))
                {
                    MessageBox.Show("Email invalido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }
    }

}
